using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseMcp.Core.Interfaces;
using DatabaseMcp.Core.Models;
using Microsoft.Extensions.Logging;

namespace DatabaseMcp.Core.Services
{
    public class PerformanceAnalyticsService : IPerformanceAnalyticsService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<PerformanceAnalyticsService> _logger;
        private readonly string _owner;

        public PerformanceAnalyticsService(
            IDbConnectionFactory connectionFactory,
            ILogger<PerformanceAnalyticsService> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger;
            _owner = Environment.GetEnvironmentVariable("SchemaOwner");
        }

        public async Task<List<SqlPerformanceMetrics>> GetTopSqlByPerformanceAsync(PerformanceAnalysisRequest request)
        {
            _logger.LogInformation("Getting top SQL by performance metrics");

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            string sql = BuildTopSqlQuery(request);

            try
            {
                using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();
                using IDbCommand command = connection.CreateCommand();
                command.CommandText = sql;

                AddSqlParameters(command, request);

                using IDataReader reader = command.ExecuteReader();
                List<SqlPerformanceMetrics> results = [];

                while (reader.Read())
                {
                    results.Add(MapSqlPerformanceMetrics(reader));
                }

                _logger.LogInformation("Retrieved {Count} SQL performance records", results.Count);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top SQL performance metrics");
                throw;
            }
        }

        public async Task<List<SqlPerformanceMetrics>> GetTopSqlByExecutionsAsync(int topN = 10, DateTime? startTime = null, DateTime? endTime = null)
        {
            _logger.LogInformation("Getting top SQL by executions");

            PerformanceAnalysisRequest request = new()
            {
                TopN = topN,
                StartTime = startTime,
                EndTime = endTime,
                OrderBy = "EXECUTIONS",
                SchemaName = _owner
            };

            return await GetTopSqlByPerformanceAsync(request);
        }

        public async Task<List<SqlPerformanceMetrics>> GetTopSqlByCpuTimeAsync(int topN = 10, DateTime? startTime = null, DateTime? endTime = null)
        {
            _logger.LogInformation("Getting top SQL by CPU time");

            PerformanceAnalysisRequest request = new()
            {
                TopN = topN,
                StartTime = startTime,
                EndTime = endTime,
                OrderBy = "CPU_TIME",
                SchemaName = _owner
            };

            return await GetTopSqlByPerformanceAsync(request);
        }

        public async Task<List<SqlPerformanceMetrics>> GetTopSqlByElapsedTimeAsync(int topN = 10, DateTime? startTime = null, DateTime? endTime = null)
        {
            _logger.LogInformation("Getting top SQL by elapsed time");

            PerformanceAnalysisRequest request = new()
            {
                TopN = topN,
                StartTime = startTime,
                EndTime = endTime,
                OrderBy = "ELAPSED_TIME",
                SchemaName = _owner
            };

            return await GetTopSqlByPerformanceAsync(request);
        }

        public async Task<List<WaitEventMetrics>> GetWaitEventAnalysisAsync(string objectName = null, DateTime? startTime = null, DateTime? endTime = null)
        {
            _logger.LogInformation("Getting wait event analysis");

            string sql = BuildWaitEventQuery(objectName);

            try
            {
                using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();
                using IDbCommand command = connection.CreateCommand();
                command.CommandText = sql;

                AddWaitEventParameters(command, objectName);

                using IDataReader reader = command.ExecuteReader();
                List<WaitEventMetrics> results = [];

                while (reader.Read())
                {
                    results.Add(MapWaitEventMetrics(reader));
                }

                _logger.LogInformation("Retrieved {Count} wait event records", results.Count);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving wait event analysis");
                throw;
            }
        }

        public async Task<List<TableUsageStatistics>> GetTableUsageStatisticsAsync(List<string> tableNames)
        {
            _logger.LogInformation("Getting table usage statistics for {Count} tables", tableNames?.Count ?? 0);

            if (tableNames == null || !tableNames.Any())
            {
                return [];
            }

            string sql = BuildTableUsageQuery(tableNames);

            try
            {
                using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();
                using IDbCommand command = connection.CreateCommand();
                command.CommandText = sql;

                AddTableNameParameters(command, tableNames);

                using IDataReader reader = command.ExecuteReader();
                List<TableUsageStatistics> results = [];

                while (reader.Read())
                {
                    results.Add(MapTableUsageStatistics(reader));
                }

                _logger.LogInformation("Retrieved {Count} table usage records", results.Count);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving table usage statistics");
                throw;
            }
        }

        public async Task<List<IndexUsageStatistics>> GetIndexUsageStatisticsAsync(string tableName)
        {
            _logger.LogInformation("Getting index usage statistics for table: {TableName}", tableName);

            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("Table name cannot be null or empty", nameof(tableName));
            }

            const string sql = @"
                SELECT 
                    i.INDEX_NAME,
                    i.TABLE_NAME,
                    i.OWNER as SCHEMA_NAME,
                    i.INDEX_TYPE,
                    i.UNIQUENESS,
                    0 as TOTAL_ACCESS,
                    SYSDATE - 365 as LAST_USED,
                    i.BLEVEL,
                    i.LEAF_BLOCKS,
                    i.DISTINCT_KEYS,
                    i.CLUSTERING_FACTOR,
                    i.STATUS,
                    LISTAGG(ic.COLUMN_NAME, ',') WITHIN GROUP (ORDER BY ic.COLUMN_POSITION) as COLUMN_NAMES,
                    'N' as IS_UNUSED,
                    SYSDATE - 30 as MONITORING_STARTED
                FROM ALL_INDEXES i
                LEFT JOIN ALL_IND_COLUMNS ic ON ic.INDEX_NAME = i.INDEX_NAME AND ic.INDEX_OWNER = i.OWNER
                WHERE i.TABLE_NAME = :tableName 
                  AND i.OWNER = :Owner
                GROUP BY i.INDEX_NAME, i.TABLE_NAME, i.OWNER, i.INDEX_TYPE, i.UNIQUENESS, 
                         i.BLEVEL, i.LEAF_BLOCKS, i.DISTINCT_KEYS, i.CLUSTERING_FACTOR, i.STATUS
                ORDER BY i.INDEX_NAME";

            try
            {
                using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();
                using IDbCommand command = connection.CreateCommand();
                command.CommandText = sql;

                IDbDataParameter parameter = command.CreateParameter();
                parameter.ParameterName = "tableName";
                parameter.Value = tableName.ToUpper();
                _ = command.Parameters.Add(parameter);
                IDbDataParameter ownerParam = command.CreateParameter();
                ownerParam.ParameterName = "Owner";
                ownerParam.Value = _owner;
                _ = command.Parameters.Add(ownerParam);

                using IDataReader reader = command.ExecuteReader();
                List<IndexUsageStatistics> results = [];

                while (reader.Read())
                {
                    results.Add(MapIndexUsageStatistics(reader));
                }

                _logger.LogInformation("Retrieved {Count} index usage records for table {TableName}", results.Count, tableName);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving index usage statistics for table {TableName}", tableName);
                throw;
            }
        }

        public async Task<List<IndexUsageStatistics>> GetUnusedIndexesAsync(List<string> tableNames = null)
        {
            string tableFilter = tableNames?.Any() == true ? $"for tables: {string.Join(", ", tableNames)}" : "for all tables";
            _logger.LogInformation("Getting unused indexes {TableFilter}", tableFilter);
            string sql = BuildUnusedIndexesQuery(tableNames);

            try
            {
                using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();
                using IDbCommand command = connection.CreateCommand();
                command.CommandText = sql;

                if (tableNames?.Any() == true)
                {
                    AddTableNameParameters(command, tableNames);
                }
                else
                {
                    // Add owner parameter for queries without table names
                    IDbDataParameter ownerParam = command.CreateParameter();
                    ownerParam.ParameterName = "Owner";
                    ownerParam.Value = _owner;
                    _ = command.Parameters.Add(ownerParam);
                }

                using IDataReader reader = command.ExecuteReader();
                List<IndexUsageStatistics> results = [];

                while (reader.Read())
                {
                    results.Add(MapIndexUsageStatistics(reader));
                }

                _logger.LogInformation("Retrieved {Count} unused indexes {TableFilter}", results.Count, tableFilter);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unused indexes {TableFilter}", tableFilter);
                throw;
            }
        }

        // Private helper methods for building SQL queries and mapping results
        private static string BuildTopSqlQuery(PerformanceAnalysisRequest request)
        {
            StringBuilder sql = new(@"
                SELECT * FROM (
                    SELECT 
                        s.SQL_ID,
                        SUBSTR(s.SQL_TEXT, 1, 1000) as SQL_TEXT,
                        s.EXECUTIONS,
                        s.ELAPSED_TIME / 1000000 as ELAPSED_TIME_SECONDS,
                        s.CPU_TIME / 1000000 as CPU_TIME_SECONDS,
                        s.DISK_READS,
                        s.BUFFER_GETS,
                        CASE WHEN s.EXECUTIONS > 0 THEN s.ELAPSED_TIME / s.EXECUTIONS / 1000000 ELSE 0 END as AVG_ELAPSED_TIME_SECONDS,
                        CASE WHEN s.EXECUTIONS > 0 THEN s.CPU_TIME / s.EXECUTIONS / 1000000 ELSE 0 END as AVG_CPU_TIME_SECONDS,
                        s.ROWS_PROCESSED,
                        s.FIRST_LOAD_TIME,
                        s.LAST_LOAD_TIME,
                        s.PLAN_HASH_VALUE,
                        s.MODULE,
                        s.PARSING_SCHEMA_NAME
                    FROM V$SQL s
                    WHERE s.EXECUTIONS > 0");

            if (request.StartTime.HasValue)
            {
                _ = sql.Append(" AND TO_DATE(s.FIRST_LOAD_TIME, 'YYYY-MM-DD/HH24:MI:SS') >= TO_DATE(:startTime, 'YYYY-MM-DD HH24:MI:SS')");
            }

            if (request.EndTime.HasValue)
            {
                _ = sql.Append(" AND TO_DATE(s.LAST_LOAD_TIME, 'YYYY-MM-DD/HH24:MI:SS') <= TO_DATE(:endTime, 'YYYY-MM-DD HH24:MI:SS')");
            }

            if (!string.IsNullOrEmpty(request.SchemaName))
            {
                _ = sql.Append(" AND s.PARSING_SCHEMA_NAME = :schemaName");
            }

            if (!request.IncludeSystemSql)
            {
                _ = sql.Append(" AND s.PARSING_SCHEMA_NAME NOT IN ('SYS', 'SYSTEM', 'DBSNMP')");
            }

            _ = sql.Append($" ORDER BY {GetOrderByClause(request.OrderBy)} DESC");
            _ = sql.Append($") WHERE ROWNUM <= {request.TopN}");

            return sql.ToString();
        }

        private static string GetOrderByClause(string orderBy)
        {
            return orderBy.ToUpper() switch
            {
                "EXECUTIONS" => "s.EXECUTIONS",
                "CPU_TIME" => "s.CPU_TIME",
                "DISK_READS" => "s.DISK_READS",
                "BUFFER_GETS" => "s.BUFFER_GETS",
                _ => "s.ELAPSED_TIME"
            };
        }

        private static void AddSqlParameters(IDbCommand command, PerformanceAnalysisRequest request)
        {
            if (request.StartTime.HasValue)
            {
                IDbDataParameter param = command.CreateParameter();
                param.ParameterName = "startTime";
                param.Value = request.StartTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                _ = command.Parameters.Add(param);
            }

            if (request.EndTime.HasValue)
            {
                IDbDataParameter param = command.CreateParameter();
                param.ParameterName = "endTime";
                param.Value = request.EndTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                _ = command.Parameters.Add(param);
            }

            if (!string.IsNullOrEmpty(request.SchemaName))
            {
                IDbDataParameter param = command.CreateParameter();
                param.ParameterName = "schemaName";
                param.Value = request.SchemaName.ToUpper();
                _ = command.Parameters.Add(param);
            }
        }

        private static SqlPerformanceMetrics MapSqlPerformanceMetrics(IDataReader reader)
        {
            return new SqlPerformanceMetrics
            {
                SqlId = SafeGetValue(reader, "SQL_ID")?.ToString() ?? string.Empty,
                SqlText = SafeGetValue(reader, "SQL_TEXT")?.ToString() ?? string.Empty,
                Executions = SafeConvertToInt64(SafeGetValue(reader, "EXECUTIONS")),
                ElapsedTimeSeconds = SafeConvertToDouble(SafeGetValue(reader, "ELAPSED_TIME_SECONDS")),
                CpuTimeSeconds = SafeConvertToDouble(SafeGetValue(reader, "CPU_TIME_SECONDS")),
                DiskReads = SafeConvertToInt64(SafeGetValue(reader, "DISK_READS")),
                BufferGets = SafeConvertToInt64(SafeGetValue(reader, "BUFFER_GETS")),
                AvgElapsedTimeSeconds = SafeConvertToDouble(SafeGetValue(reader, "AVG_ELAPSED_TIME_SECONDS")),
                AvgCpuTimeSeconds = SafeConvertToDouble(SafeGetValue(reader, "AVG_CPU_TIME_SECONDS")),
                RowsProcessed = SafeConvertToInt64(SafeGetValue(reader, "ROWS_PROCESSED")),
                FirstLoadTime = ParseOracleDate(SafeGetValue(reader, "FIRST_LOAD_TIME")?.ToString()),
                LastLoadTime = ParseOracleDate(SafeGetValue(reader, "LAST_LOAD_TIME")?.ToString()),
                PlanHash = SafeGetValue(reader, "PLAN_HASH_VALUE")?.ToString() ?? string.Empty,
                Module = SafeGetValue(reader, "MODULE")?.ToString() ?? string.Empty,
                ParsingSchemaName = SafeGetValue(reader, "PARSING_SCHEMA_NAME")?.ToString() ?? string.Empty
            };
        }

        private static DateTime ParseOracleDate(string dateString)
        {
            return string.IsNullOrEmpty(dateString)
                ? DateTime.MinValue
                : DateTime.TryParseExact(dateString, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result)
                ? result
                : DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out result) ? result : DateTime.MinValue;
        }

        private static long SafeConvertToInt64(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return 0;
            }

            try
            {
                // Handle decimal values from Oracle
                if (value is decimal decimalValue)
                {
                    return decimalValue > long.MaxValue ? long.MaxValue : decimalValue < long.MinValue ? long.MinValue : Convert.ToInt64(decimalValue);
                }

                string stringValue = value.ToString();
                if (string.IsNullOrEmpty(stringValue))
                {
                    return 0;
                }

                if (long.TryParse(stringValue, out long result))
                {
                    return result;
                }

                // Try parsing as decimal first, then convert
                return decimal.TryParse(stringValue, out decimal decResult)
                    ? decResult > long.MaxValue ? long.MaxValue : decResult < long.MinValue ? long.MinValue : Convert.ToInt64(decResult)
                    : 0;
            }
            catch (OverflowException)
            {
                return long.MaxValue;
            }
            catch
            {
                return 0;
            }
        }

        private static double SafeConvertToDouble(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return 0.0;
            }

            try
            {
                // Handle decimal values from Oracle
                if (value is decimal decimalValue)
                {
                    return Convert.ToDouble(decimalValue);
                }

                string stringValue = value.ToString();
                return string.IsNullOrEmpty(stringValue)
                    ? 0.0
                    : double.TryParse(stringValue, out double result) ? double.IsInfinity(result) || double.IsNaN(result) ? 0.0 : result : 0.0;
            }
            catch (OverflowException)
            {
                return double.MaxValue;
            }
            catch
            {
                return 0.0;
            }
        }

        private static int SafeConvertToInt32(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return 0;
            }

            try
            {
                // Handle decimal values from Oracle
                if (value is decimal decimalValue)
                {
                    return decimalValue > int.MaxValue ? int.MaxValue : decimalValue < int.MinValue ? int.MinValue : Convert.ToInt32(decimalValue);
                }

                string stringValue = value.ToString();
                if (string.IsNullOrEmpty(stringValue))
                {
                    return 0;
                }

                if (int.TryParse(stringValue, out int result))
                {
                    return result;
                }

                // Try parsing as decimal first, then convert
                return decimal.TryParse(stringValue, out decimal decResult)
                    ? decResult > int.MaxValue ? int.MaxValue : decResult < int.MinValue ? int.MinValue : Convert.ToInt32(decResult)
                    : 0;
            }
            catch (OverflowException)
            {
                return int.MaxValue;
            }
            catch
            {
                return 0;
            }
        }

        private static object SafeGetValue(IDataReader reader, string columnName)
        {
            try
            {
                return reader[columnName];
            }
            catch (InvalidCastException)
            {
                // Handle Oracle decimal overflow by trying to get as string
                try
                {
                    int ordinal = reader.GetOrdinal(columnName);
                    if (reader.IsDBNull(ordinal))
                    {
                        return DBNull.Value;
                    }

                    // Try to get as string to avoid decimal overflow
                    return reader.GetString(ordinal);
                }
                catch
                {
                    return DBNull.Value;
                }
            }
            catch (OverflowException)
            {
                // Handle Oracle arithmetic overflow
                try
                {
                    int ordinal = reader.GetOrdinal(columnName);
                    if (reader.IsDBNull(ordinal))
                    {
                        return DBNull.Value;
                    }

                    // Try to get as string to avoid overflow
                    return reader.GetString(ordinal);
                }
                catch
                {
                    return DBNull.Value;
                }
            }
            catch
            {
                return DBNull.Value;
            }
        }

        private static string BuildWaitEventQuery(string objectName)
        {
            StringBuilder sql = new(@"
                SELECT 
                    w.EVENT as EVENT_NAME,
                    w.WAIT_CLASS,
                    w.TOTAL_WAITS,
                    w.TIME_WAITED / 100 as TOTAL_WAIT_TIME_SECONDS,
                    CASE WHEN w.TOTAL_WAITS > 0 THEN (w.TIME_WAITED / w.TOTAL_WAITS) / 10 ELSE 0 END as AVG_WAIT_TIME_MS,
                    w.TIME_WAITED_MICRO / (SELECT SUM(TIME_WAITED_MICRO) FROM V$SYSTEM_EVENT) * 100 as TIME_WAITED_PERCENT,
                    NVL(o.OBJECT_NAME, '') as DESCRIPTION,
                    SYSDATE as SAMPLE_TIME,
                    NVL(o.OBJECT_NAME, '') as OBJECT_NAME,
                    NVL(o.OWNER, '') as OBJECT_OWNER
                FROM V$SYSTEM_EVENT w
                LEFT JOIN (
                    SELECT DISTINCT 
                        obj.OBJECT_NAME,
                        obj.OWNER,
                        CASE 
                            WHEN obj.OBJECT_TYPE = 'TABLE' THEN obj.OBJECT_NAME
                            WHEN obj.OBJECT_TYPE = 'INDEX' THEN obj.OBJECT_NAME
                            ELSE obj.OBJECT_NAME
                        END as RELATED_OBJECT
                    FROM ALL_OBJECTS obj
                    WHERE obj.OWNER = :Owner
                      AND obj.OBJECT_TYPE IN ('TABLE', 'INDEX', 'VIEW', 'SEQUENCE', 'PROCEDURE', 'FUNCTION', 'PACKAGE')
                ) o ON (w.EVENT LIKE '%' || o.RELATED_OBJECT || '%' OR w.EVENT LIKE '%' || o.OBJECT_NAME || '%')
                WHERE w.EVENT NOT LIKE 'SQL*Net%'
                  AND w.EVENT NOT LIKE '%idle%'
                  AND w.TIME_WAITED > 0");

            if (!string.IsNullOrEmpty(objectName))
            {
                _ = sql.Append(" AND (w.EVENT LIKE '%' || :objectName || '%' OR o.OBJECT_NAME LIKE '%' || :objectName || '%')");
            }

            // Filter to show only wait events that are likely related to our schema objects
            _ = sql.Append(" AND (o.OWNER = :Owner OR w.EVENT IN ('db file sequential read', 'db file scattered read', 'direct path read', 'direct path write'))");

            _ = sql.Append(" ORDER BY w.TIME_WAITED DESC");

            return sql.ToString();
        }

        private void AddWaitEventParameters(IDbCommand command, string objectName)
        {
            // Always add owner parameter for schema filtering
            IDbDataParameter ownerParam = command.CreateParameter();
            ownerParam.ParameterName = "Owner";
            ownerParam.Value = _owner;
            _ = command.Parameters.Add(ownerParam);

            if (!string.IsNullOrEmpty(objectName))
            {
                IDbDataParameter param = command.CreateParameter();
                param.ParameterName = "objectName";
                param.Value = objectName.ToUpper();
                _ = command.Parameters.Add(param);
            }
        }

        private static WaitEventMetrics MapWaitEventMetrics(IDataReader reader)
        {
            return new WaitEventMetrics
            {
                EventName = SafeGetValue(reader, "EVENT_NAME")?.ToString() ?? string.Empty,
                WaitClass = SafeGetValue(reader, "WAIT_CLASS")?.ToString() ?? string.Empty,
                TotalWaits = SafeConvertToInt64(SafeGetValue(reader, "TOTAL_WAITS")),
                TotalWaitTimeSeconds = SafeConvertToDouble(SafeGetValue(reader, "TOTAL_WAIT_TIME_SECONDS")),
                AvgWaitTimeMs = SafeConvertToDouble(SafeGetValue(reader, "AVG_WAIT_TIME_MS")),
                TimeWaitedPercent = SafeConvertToDouble(SafeGetValue(reader, "TIME_WAITED_PERCENT")),
                Description = SafeGetValue(reader, "DESCRIPTION")?.ToString() ?? string.Empty,
                SampleTime = SafeGetValue(reader, "SAMPLE_TIME") != DBNull.Value ? Convert.ToDateTime(SafeGetValue(reader, "SAMPLE_TIME"), CultureInfo.InvariantCulture) : DateTime.Now,
                ObjectName = SafeGetValue(reader, "OBJECT_NAME")?.ToString() ?? string.Empty,
                ObjectOwner = SafeGetValue(reader, "OBJECT_OWNER")?.ToString() ?? string.Empty
            };
        }

        private static string BuildTableUsageQuery(List<string> tableNames)
        {
            string inClause = string.Join(",", tableNames.Select((_, i) => $":p{i}"));

            return $@"
                SELECT 
                    t.TABLE_NAME,
                    t.OWNER as SCHEMA_NAME,
                    0 as TABLE_SCANS,
                    0 as ROW_LOOKUPS,
                    NVL(dm.INSERTS, 0) as ROWS_INSERTED,
                    NVL(dm.UPDATES, 0) as ROWS_UPDATED,
                    NVL(dm.DELETES, 0) as ROWS_DELETED,
                    NVL(dm.TIMESTAMP, SYSDATE - 365) as LAST_USED,
                    NVL(t.AVG_ROW_LEN, 0) as AVG_ROW_LENGTH,
                    NVL(t.NUM_ROWS, 0) * NVL(t.AVG_ROW_LEN, 0) as TABLE_SIZE,
                    NVL(t.NUM_ROWS, 0) as NUM_ROWS,
                    NVL(t.TABLESPACE_NAME, 'UNKNOWN') as TABLESPACE_NAME,
                    CASE WHEN pt.TABLE_NAME IS NOT NULL THEN 1 ELSE 0 END as IS_PARTITIONED
                FROM ALL_TABLES t
                LEFT JOIN DBA_TAB_MODIFICATIONS dm ON dm.TABLE_NAME = t.TABLE_NAME AND dm.TABLE_OWNER = t.OWNER
                LEFT JOIN ALL_PART_TABLES pt ON pt.TABLE_NAME = t.TABLE_NAME AND pt.OWNER = t.OWNER
                WHERE t.TABLE_NAME IN ({inClause})
                  AND t.OWNER = :Owner
                ORDER BY t.TABLE_NAME";
        }

        private static void AddTableNameParameters(IDbCommand command, List<string> tableNames)
        {
            for (int i = 0; i < tableNames.Count; i++)
            {
                IDbDataParameter param = command.CreateParameter();
                param.ParameterName = $"p{i}";
                param.Value = tableNames[i].ToUpper();
                _ = command.Parameters.Add(param);
            }
            
            // Add owner parameter
            IDbDataParameter ownerParam = command.CreateParameter();
            ownerParam.ParameterName = "Owner";
            ownerParam.Value = Environment.GetEnvironmentVariable("SchemaOwner");
            _ = command.Parameters.Add(ownerParam);
        }

        private string BuildUnusedIndexesQuery(List<string> tableNames)
        {
            StringBuilder sql = new(@"
                SELECT 
                    i.INDEX_NAME,
                    i.TABLE_NAME,
                    i.OWNER AS SCHEMA_NAME,
                    i.INDEX_TYPE,
                    i.UNIQUENESS,
                    0 AS TOTAL_ACCESS,
                    NVL(i.LAST_ANALYZED, SYSDATE - 365) AS LAST_USED,
                    i.BLEVEL,
                    i.LEAF_BLOCKS,
                    i.DISTINCT_KEYS,
                    i.CLUSTERING_FACTOR,
                    i.STATUS,
                    LISTAGG(ic.COLUMN_NAME, ',') 
                        WITHIN GROUP (ORDER BY ic.COLUMN_POSITION) AS COLUMN_NAMES,
                    'Y' AS IS_UNUSED
                FROM 
                    ALL_INDEXES i
                    LEFT JOIN ALL_IND_COLUMNS ic 
                        ON ic.INDEX_NAME = i.INDEX_NAME 
                        AND ic.INDEX_OWNER = i.OWNER
                        AND ic.TABLE_NAME = i.TABLE_NAME
                        AND ic.TABLE_OWNER = i.TABLE_OWNER
                WHERE 
                    i.OWNER = :Owner
                    AND i.INDEX_TYPE != 'LOB'
                    AND i.UNIQUENESS = 'NONUNIQUE'
                    AND i.STATUS = 'VALID'");

            // Add table name filter if specified
            if (tableNames?.Any() == true)
            {
                string inClause = string.Join(",", tableNames.Select((_, i) => $":p{i}"));
                _ = sql.Append($" AND i.TABLE_NAME IN ({inClause})");
            }

            _ = sql.Append(@"
                GROUP BY 
                    i.INDEX_NAME,
                    i.TABLE_NAME,
                    i.OWNER,
                    i.INDEX_TYPE,
                    i.UNIQUENESS,
                    i.LAST_ANALYZED,
                    i.BLEVEL,
                    i.LEAF_BLOCKS,
                    i.DISTINCT_KEYS,
                    i.CLUSTERING_FACTOR,
                    i.STATUS
                ORDER BY i.TABLE_NAME, i.INDEX_NAME");
            _logger.LogInformation(sql.ToString());
            return sql.ToString();
        }

        private static TableUsageStatistics MapTableUsageStatistics(IDataReader reader)
        {
            return new TableUsageStatistics
            {
                TableName = SafeGetValue(reader, "TABLE_NAME")?.ToString() ?? string.Empty,
                SchemaName = SafeGetValue(reader, "SCHEMA_NAME")?.ToString() ?? string.Empty,
                TableScans = SafeConvertToInt64(SafeGetValue(reader, "TABLE_SCANS")),
                RowLookups = SafeConvertToInt64(SafeGetValue(reader, "ROW_LOOKUPS")),
                RowsInserted = SafeConvertToInt64(SafeGetValue(reader, "ROWS_INSERTED")),
                RowsUpdated = SafeConvertToInt64(SafeGetValue(reader, "ROWS_UPDATED")),
                RowsDeleted = SafeConvertToInt64(SafeGetValue(reader, "ROWS_DELETED")),
                LastUsed = SafeGetValue(reader, "LAST_USED") != DBNull.Value ? Convert.ToDateTime(SafeGetValue(reader, "LAST_USED"), CultureInfo.InvariantCulture) : DateTime.Now,
                AvgRowLength = SafeConvertToDouble(SafeGetValue(reader, "AVG_ROW_LENGTH")),
                TableSize = SafeConvertToInt64(SafeGetValue(reader, "TABLE_SIZE")),
                NumRows = SafeConvertToInt64(SafeGetValue(reader, "NUM_ROWS")),
                Tablespace = SafeGetValue(reader, "TABLESPACE_NAME")?.ToString() ?? string.Empty,
                IsPartitioned = SafeConvertToInt32(SafeGetValue(reader, "IS_PARTITIONED")) == 1
            };
        }

        private static IndexUsageStatistics MapIndexUsageStatistics(IDataReader reader)
        {
            string[] columnNames = SafeGetValue(reader, "COLUMN_NAMES")?.ToString()?.Split(',') ?? Array.Empty<string>();

            return new IndexUsageStatistics
            {
                IndexName = SafeGetValue(reader, "INDEX_NAME")?.ToString() ?? string.Empty,
                TableName = SafeGetValue(reader, "TABLE_NAME")?.ToString() ?? string.Empty,
                SchemaName = SafeGetValue(reader, "SCHEMA_NAME")?.ToString() ?? string.Empty,
                IndexType = SafeGetValue(reader, "INDEX_TYPE")?.ToString() ?? string.Empty,
                IsUnique = SafeGetValue(reader, "UNIQUENESS")?.ToString() == "UNIQUE",
                TotalAccess = SafeConvertToInt64(SafeGetValue(reader, "TOTAL_ACCESS")),
                LastUsed = SafeGetValue(reader, "LAST_USED") != DBNull.Value ? Convert.ToDateTime(SafeGetValue(reader, "LAST_USED"), CultureInfo.InvariantCulture) : DateTime.Now.AddDays(-365),
                BlevelDepth = SafeConvertToInt64(SafeGetValue(reader, "BLEVEL")),
                LeafBlocks = SafeConvertToInt64(SafeGetValue(reader, "LEAF_BLOCKS")),
                DistinctKeys = SafeConvertToInt64(SafeGetValue(reader, "DISTINCT_KEYS")),
                ClusteringFactor = SafeConvertToDouble(SafeGetValue(reader, "CLUSTERING_FACTOR")),
                Status = SafeGetValue(reader, "STATUS")?.ToString() ?? string.Empty,
                ColumnNames = columnNames.ToList(),
                IsUnused = SafeGetValue(reader, "IS_UNUSED")?.ToString() == "Y",
                MonitoringStarted = SafeGetValue(reader, "MONITORING_STARTED") != DBNull.Value ? Convert.ToDateTime(SafeGetValue(reader, "MONITORING_STARTED"), CultureInfo.InvariantCulture) : DateTime.Now.AddDays(-30)
            };
        }
    }
}

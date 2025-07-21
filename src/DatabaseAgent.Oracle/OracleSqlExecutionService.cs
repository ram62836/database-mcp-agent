using Hala.DatabaseAgent.Core.Interfaces;
using Hala.DatabaseAgent.Core.Models;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Diagnostics;

namespace Hala.DatabaseAgent.Oracle
{
    /// <summary>
    /// Oracle implementation of SQL execution service
    /// </summary>
    public class OracleSqlExecutionService : ISqlExecutionService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<OracleSqlExecutionService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OracleSqlExecutionService"/> class.
        /// </summary>
        /// <param name="connectionFactory">The connection factory.</param>
        /// <param name="logger">The logger.</param>
        public OracleSqlExecutionService(IDbConnectionFactory connectionFactory, ILogger<OracleSqlExecutionService> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<SqlQueryResult> ExecuteQueryAsync(string sql, int? timeoutSeconds = null)
        {
            var result = new SqlQueryResult();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync();
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                
                if (timeoutSeconds.HasValue)
                {
                    command.CommandTimeout = timeoutSeconds.Value;
                }

                // Check if the SQL is a SELECT statement (roughly)
                bool isQuery = sql.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) ||
                               sql.TrimStart().StartsWith("WITH", StringComparison.OrdinalIgnoreCase);

                if (isQuery)
                {
                    using var reader = await ((OracleCommand)command).ExecuteReaderAsync();
                    
                    // Get column information
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        result.ColumnNames.Add(reader.GetName(i));
                        result.ColumnTypes.Add(reader.GetFieldType(i)?.Name ?? "unknown");
                    }

                    // Get data
                    while (await reader.ReadAsync())
                    {
                        var row = new List<object?>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row.Add(reader.IsDBNull(i) ? null : reader.GetValue(i));
                        }
                        result.Rows.Add(row);
                    }
                }
                else
                {
                    // For non-query statements, execute and get rows affected
                    result.RowsAffected = await ((OracleCommand)command).ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Error executing Oracle SQL: {Sql}", sql);
            }
            finally
            {
                stopwatch.Stop();
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
            }

            return result;
        }

        /// <inheritdoc/>
        public SqlQueryResult ExecuteQuery(string sql, int? timeoutSeconds = null)
        {
            var result = new SqlQueryResult();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using var connection = _connectionFactory.CreateConnection();
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                
                if (timeoutSeconds.HasValue)
                {
                    command.CommandTimeout = timeoutSeconds.Value;
                }

                // Check if the SQL is a SELECT statement (roughly)
                bool isQuery = sql.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) ||
                               sql.TrimStart().StartsWith("WITH", StringComparison.OrdinalIgnoreCase);

                if (isQuery)
                {
                    using var reader = command.ExecuteReader();
                    
                    // Get column information
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        result.ColumnNames.Add(reader.GetName(i));
                        result.ColumnTypes.Add(reader.GetFieldType(i)?.Name ?? "unknown");
                    }

                    // Get data
                    while (reader.Read())
                    {
                        var row = new List<object?>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row.Add(reader.IsDBNull(i) ? null : reader.GetValue(i));
                        }
                        result.Rows.Add(row);
                    }
                }
                else
                {
                    // For non-query statements, execute and get rows affected
                    result.RowsAffected = command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Error executing Oracle SQL: {Sql}", sql);
            }
            finally
            {
                stopwatch.Stop();
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
            }

            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DatabaseMcp.Core.Interfaces;
using DatabaseMcp.Core.Models;

namespace DatabaseMcp.Core.Services
{
    public class ConstraintGatheringService : IConstraintGatheringService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<ConstraintGatheringService> _logger;

        public ConstraintGatheringService(IDbConnectionFactory connectionFactory, ILogger<ConstraintGatheringService> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger;
        }

        public async Task<List<ConstraintMetadata>> GetUniqueConstraintsAsync(string tableName)
        {
            _logger.LogInformation("Getting unique constraints for table: {TableName}", tableName);
            List<ConstraintMetadata> uniqueConstraints = new();
            try
            {
                using (IDbConnection connection = await _connectionFactory.CreateConnectionAsync())
                {
                    string query = @"SELECT AC.CONSTRAINT_NAME, COLUMN_NAME FROM ALL_CONS_COLUMNS ACC JOIN ALL_CONSTRAINTS AC ON ACC.CONSTRAINT_NAME = AC.CONSTRAINT_NAME WHERE ACC.TABLE_NAME = :TableName AND AC.CONSTRAINT_TYPE = 'U' AND AC.OWNER NOT IN ('SYS', 'SYSTEM', 'XDB', 'OUTLN', 'CTXSYS', 'DBSNMP', 'ORDDATA', 'ORDSYS', 'MDSYS', 'WMSYS', 'OLAPSYS', 'EXFSYS', 'SYSMAN', 'APEX_040000', 'FLOWS_FILES') AND AC.OWNER NOT LIKE '%SYS%'";

                    using IDbCommand command = connection.CreateCommand();
                    command.CommandText = query;
                    IDbDataParameter param = command.CreateParameter();
                    param.ParameterName = "TableName";
                    param.Value = tableName.ToUpper();
                    _ = command.Parameters.Add(param);

                    using IDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        uniqueConstraints.Add(new ConstraintMetadata
                        {
                            ConstraintName = reader["CONSTRAINT_NAME"].ToString(),
                            ColumnName = reader["COLUMN_NAME"].ToString(),
                            ConstraintType = "Unique"
                        });
                    }
                }
                _logger.LogInformation("Retrieved {Count} unique constraints for table {TableName}", uniqueConstraints.Count, tableName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unique constraints for table: {TableName}", tableName);
                throw;
            }
            return uniqueConstraints;
        }

        public async Task<List<ConstraintMetadata>> GetCheckConstraintsAsync(string tableName)
        {
            _logger.LogInformation("Getting check constraints for table: {TableName}", tableName);
            List<ConstraintMetadata> checkConstraints = new();
            try
            {
                using (IDbConnection connection = await _connectionFactory.CreateConnectionAsync())
                {
                    string query = @"SELECT CONSTRAINT_NAME, SEARCH_CONDITION FROM ALL_CONSTRAINTS WHERE TABLE_NAME = :TableName AND CONSTRAINT_TYPE = 'C'";

                    using IDbCommand command = connection.CreateCommand();
                    command.CommandText = query;
                    IDbDataParameter param = command.CreateParameter();
                    param.ParameterName = "TableName";
                    param.Value = tableName.ToUpper();
                    _ = command.Parameters.Add(param);

                    using IDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        checkConstraints.Add(new ConstraintMetadata
                        {
                            ConstraintName = reader["CONSTRAINT_NAME"].ToString(),
                            SearchCondition = reader["SEARCH_CONDITION"].ToString(),
                            ConstraintType = "Check"
                        });
                    }
                }
                _logger.LogInformation("Retrieved {Count} check constraints for table {TableName}", checkConstraints.Count, tableName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting check constraints for table: {TableName}", tableName);
                throw;
            }
            return checkConstraints;
        }
    }
}
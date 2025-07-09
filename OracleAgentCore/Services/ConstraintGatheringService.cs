using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using OracleAgent.Core.Models;
using OracleAgent.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace OracleAgent.Core.Services
{
    public class ConstraintGatheringService : IConstraintGatheringService
    {
        private readonly string _connectionString;
        private readonly ILogger<ConstraintGatheringService> _logger;

        public ConstraintGatheringService(IConfiguration config, ILogger<ConstraintGatheringService> logger)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public async Task<List<ConstraintMetadata>> GetUniqueConstraintsAsync(string tableName)
        {
            _logger.LogInformation("Getting unique constraints for table: {TableName}", tableName);
            var uniqueConstraints = new List<ConstraintMetadata>();
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = @"SELECT AC.CONSTRAINT_NAME, COLUMN_NAME
                                  FROM ALL_CONS_COLUMNS ACC
                                  JOIN ALL_CONSTRAINTS AC
                                  ON ACC.CONSTRAINT_NAME = AC.CONSTRAINT_NAME
                                  WHERE ACC.TABLE_NAME = :TableName
                                  AND AC.CONSTRAINT_TYPE = 'U'
                                  AND AC.OWNER NOT IN ('SYS', 'SYSTEM', 'XDB', 'OUTLN', 'CTXSYS', 'DBSNMP', 'ORDDATA', 'ORDSYS', 'MDSYS', 'WMSYS', 'OLAPSYS', 'EXFSYS', 'SYSMAN', 'APEX_040000', 'FLOWS_FILES')
                                  AND AC.OWNER NOT LIKE '%SYS%'";

                    using (var command = new OracleCommand(query, connection))
                    {
                        command.Parameters.Add(new OracleParameter("TableName", tableName.ToUpper()));

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                uniqueConstraints.Add(new ConstraintMetadata
                                {
                                    ConstraintName = reader["CONSTRAINT_NAME"].ToString(),
                                    ColumnName = reader["COLUMN_NAME"].ToString(),
                                    ConstraintType = "Unique"
                                });
                            }
                        }
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
            var checkConstraints = new List<ConstraintMetadata>();
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = @"SELECT CONSTRAINT_NAME, SEARCH_CONDITION
                                  FROM ALL_CONSTRAINTS
                                  WHERE TABLE_NAME = :TableName
                                  AND CONSTRAINT_TYPE = 'C'";

                    using (var command = new OracleCommand(query, connection))
                    {
                        command.Parameters.Add(new OracleParameter("TableName", tableName.ToUpper()));

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                checkConstraints.Add(new ConstraintMetadata
                                {
                                    ConstraintName = reader["CONSTRAINT_NAME"].ToString(),
                                    SearchCondition = reader["SEARCH_CONDITION"].ToString(),
                                    ConstraintType = "Check"
                                });
                            }
                        }
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
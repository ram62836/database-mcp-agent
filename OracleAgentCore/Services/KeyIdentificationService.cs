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
    public class KeyIdentificationService : IKeyIdentificationService
    {
        private readonly string _connectionString;
        private readonly ILogger<KeyIdentificationService> _logger;

        public KeyIdentificationService(IConfiguration config, ILogger<KeyIdentificationService> logger)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public async Task<List<KeyMetadata>> GetPrimaryKeysAsync(string tableName)
        {
            _logger.LogInformation("Getting primary keys for table: {TableName}", tableName);
            var primaryKeys = new List<KeyMetadata>();
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = @"SELECT cols.COLUMN_NAME, cons.CONSTRAINT_NAME  
                                  FROM ALL_CONS_COLUMNS cols  
                                  JOIN ALL_CONSTRAINTS cons  
                                  ON cols.CONSTRAINT_NAME = cons.CONSTRAINT_NAME  
                                  WHERE cons.CONSTRAINT_TYPE = 'P'  
                                  AND cons.TABLE_NAME = :TableName";

                    using (var command = new OracleCommand(query, connection))
                    {
                        command.Parameters.Add(new OracleParameter("TableName", tableName.ToUpper()));

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                primaryKeys.Add(new KeyMetadata
                                {
                                    ColumnName = reader["COLUMN_NAME"].ToString(),
                                    ConstraintName = reader["CONSTRAINT_NAME"].ToString(),
                                    KeyType = "Primary"
                                });
                            }
                        }
                    }
                }
                _logger.LogInformation("Retrieved {Count} primary keys for table {TableName}", primaryKeys.Count, tableName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting primary keys for table: {TableName}", tableName);
                throw;
            }
            return primaryKeys;
        }

        public async Task<List<KeyMetadata>> GetForeignKeysAsync(string tableName)
        {
            _logger.LogInformation("Getting foreign keys for table: {TableName}", tableName);
            var foreignKeys = new List<KeyMetadata>();
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = @"SELECT cols.COLUMN_NAME, cons.CONSTRAINT_NAME, cons.R_CONSTRAINT_NAME  
                                  FROM ALL_CONS_COLUMNS cols  
                                  JOIN ALL_CONSTRAINTS cons  
                                  ON cols.CONSTRAINT_NAME = cons.CONSTRAINT_NAME  
                                  WHERE cons.CONSTRAINT_TYPE = 'R'  
                                  AND cons.TABLE_NAME = :TableName";

                    using (var command = new OracleCommand(query, connection))
                    {
                        command.Parameters.Add(new OracleParameter("TableName", tableName.ToUpper()));

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                foreignKeys.Add(new KeyMetadata
                                {
                                    ColumnName = reader["COLUMN_NAME"].ToString(),
                                    ConstraintName = reader["CONSTRAINT_NAME"].ToString(),
                                    ReferencedConstraintName = reader["R_CONSTRAINT_NAME"].ToString(),
                                    KeyType = "Foreign"
                                });
                            }
                        }
                    }
                }
                _logger.LogInformation("Retrieved {Count} foreign keys for table {TableName}", foreignKeys.Count, tableName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting foreign keys for table: {TableName}", tableName);
                throw;
            }
            return foreignKeys;
        }
        
        public async Task<Dictionary<string, List<string>>> GetForeignKeyRelationshipsAsync()
        {
            _logger.LogInformation("Getting all foreign key relationships");
            var relationships = new Dictionary<string, List<string>>();
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = @"SELECT cons.CONSTRAINT_NAME, cols.TABLE_NAME, cols.COLUMN_NAME  
                                  FROM ALL_CONS_COLUMNS cols  
                                  JOIN ALL_CONSTRAINTS cons  
                                  ON cols.CONSTRAINT_NAME = cons.CONSTRAINT_NAME  
                                  WHERE cons.CONSTRAINT_TYPE = 'R'";

                    using (var command = new OracleCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var constraintName = reader["CONSTRAINT_NAME"].ToString();
                                var columnName = reader["COLUMN_NAME"].ToString();

                                if (!relationships.ContainsKey(constraintName))
                                {
                                    relationships[constraintName] = new List<string>();
                                }

                                relationships[constraintName].Add(columnName);
                            }
                        }
                    }
                }
                _logger.LogInformation("Retrieved {Count} foreign key relationships", relationships.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting foreign key relationships");
                throw;
            }
            return relationships;
        }
    }
}
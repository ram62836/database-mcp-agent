using System;
using System.Collections.Generic;
using OracleAgent.Core.Models;
using OracleAgent.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Data;

namespace OracleAgent.Core.Services
{
    public class KeyIdentificationService : IKeyIdentificationService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<KeyIdentificationService> _logger;

        public KeyIdentificationService(IDbConnectionFactory connectionFactory, ILogger<KeyIdentificationService> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger;
        }

        public async Task<List<KeyMetadata>> GetPrimaryKeysAsync(string tableName)
        {
            _logger.LogInformation("Getting primary keys for table: {TableName}", tableName);
            var primaryKeys = new List<KeyMetadata>();
            try
            {
                using (var connection = await _connectionFactory.CreateConnectionAsync())
                {
                    var query = @"SELECT cols.COLUMN_NAME, cons.CONSTRAINT_NAME FROM ALL_CONS_COLUMNS cols JOIN ALL_CONSTRAINTS cons ON cols.CONSTRAINT_NAME = cons.CONSTRAINT_NAME WHERE cons.CONSTRAINT_TYPE = 'P' AND cons.TABLE_NAME = :TableName";

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = query;
                        var param = command.CreateParameter();
                        param.ParameterName = "TableName";
                        param.Value = tableName.ToUpper();
                        command.Parameters.Add(param);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
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
                using (var connection = await _connectionFactory.CreateConnectionAsync())
                {
                    var query = @"SELECT cols.COLUMN_NAME, cons.CONSTRAINT_NAME, cons.R_CONSTRAINT_NAME FROM ALL_CONS_COLUMNS cols JOIN ALL_CONSTRAINTS cons ON cols.CONSTRAINT_NAME = cons.CONSTRAINT_NAME WHERE cons.CONSTRAINT_TYPE = 'R' AND cons.TABLE_NAME = :TableName";

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = query;
                        var param = command.CreateParameter();
                        param.ParameterName = "TableName";
                        param.Value = tableName.ToUpper();
                        command.Parameters.Add(param);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
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
                using (var connection = await _connectionFactory.CreateConnectionAsync())
                {
                    var query = @"SELECT cons.CONSTRAINT_NAME, cols.TABLE_NAME, cols.COLUMN_NAME FROM ALL_CONS_COLUMNS cols JOIN ALL_CONSTRAINTS cons ON cols.CONSTRAINT_NAME = cons.CONSTRAINT_NAME WHERE cons.CONSTRAINT_TYPE = 'R'";

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = query;
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
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
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using DatabaseMcp.Core.Interfaces;
using DatabaseMcp.Core.Models;
using Microsoft.Extensions.Logging;

namespace DatabaseMcp.Core.Services
{
    public class KeyIdentificationService : IKeyIdentificationService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<KeyIdentificationService> _logger;
        private readonly string _owner;

        public KeyIdentificationService(IDbConnectionFactory connectionFactory, ILogger<KeyIdentificationService> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger;
            _owner = Environment.GetEnvironmentVariable("SchemaOwner");
        }

        public async Task<List<KeyMetadata>> GetPrimaryKeysAsync(string tableName)
        {
            _logger.LogInformation("Getting primary keys for table: {TableName}", tableName);
            List<KeyMetadata> primaryKeys = [];
            try
            {
                using (IDbConnection connection = await _connectionFactory.CreateConnectionAsync())
                {
                    string query = @"SELECT cols.COLUMN_NAME, cons.CONSTRAINT_NAME FROM ALL_CONS_COLUMNS cols JOIN ALL_CONSTRAINTS cons ON cols.CONSTRAINT_NAME = cons.CONSTRAINT_NAME WHERE cons.CONSTRAINT_TYPE = 'P' AND cons.TABLE_NAME = :TableName AND cons.OWNER = :Owner";

                    using IDbCommand command = connection.CreateCommand();
                    command.CommandText = query;
                    IDbDataParameter param = command.CreateParameter();
                    param.ParameterName = "TableName";
                    param.Value = tableName.ToUpper();
                    _ = command.Parameters.Add(param);
                    IDbDataParameter ownerParam = command.CreateParameter();
                    ownerParam.ParameterName = "Owner";
                    ownerParam.Value = _owner;
                    _ = command.Parameters.Add(ownerParam);

                    using IDataReader reader = command.ExecuteReader();
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
            List<KeyMetadata> foreignKeys = [];
            try
            {
                using (IDbConnection connection = await _connectionFactory.CreateConnectionAsync())
                {
                    string query = @"SELECT cols.COLUMN_NAME, cons.CONSTRAINT_NAME, cons.R_CONSTRAINT_NAME FROM ALL_CONS_COLUMNS cols JOIN ALL_CONSTRAINTS cons ON cols.CONSTRAINT_NAME = cons.CONSTRAINT_NAME WHERE cons.CONSTRAINT_TYPE = 'R' AND cons.TABLE_NAME = :TableName AND cons.OWNER = :Owner";

                    using IDbCommand command = connection.CreateCommand();
                    command.CommandText = query;
                    IDbDataParameter param = command.CreateParameter();
                    param.ParameterName = "TableName";
                    param.Value = tableName.ToUpper();
                    _ = command.Parameters.Add(param);
                    IDbDataParameter ownerParam = command.CreateParameter();
                    ownerParam.ParameterName = "Owner";
                    ownerParam.Value = _owner;
                    _ = command.Parameters.Add(ownerParam);

                    using IDataReader reader = command.ExecuteReader();
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
            Dictionary<string, List<string>> relationships = [];
            try
            {
                using (IDbConnection connection = await _connectionFactory.CreateConnectionAsync())
                {
                    string query = @"SELECT cons.CONSTRAINT_NAME, cols.TABLE_NAME, cols.COLUMN_NAME FROM ALL_CONS_COLUMNS cols JOIN ALL_CONSTRAINTS cons ON cols.CONSTRAINT_NAME = cons.CONSTRAINT_NAME WHERE cons.CONSTRAINT_TYPE = 'R'";

                    using IDbCommand command = connection.CreateCommand();
                    command.CommandText = query;
                    using IDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string constraintName = reader["CONSTRAINT_NAME"].ToString();
                        string columnName = reader["COLUMN_NAME"].ToString();

                        if (!relationships.ContainsKey(constraintName))
                        {
                            relationships[constraintName] = [];
                        }

                        relationships[constraintName].Add(columnName);
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
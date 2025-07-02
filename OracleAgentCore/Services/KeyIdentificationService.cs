using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using OracleAgent.Core.Models;
using OracleAgent.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace OracleAgent.Core.Services
{
    public class KeyIdentificationService : IKeyIdentificationService
    {
        private readonly string _connectionString;

        public KeyIdentificationService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<List<KeyMetadata>> GetPrimaryKeysAsync(string tableName)
        {
            var primaryKeys = new List<KeyMetadata>();
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
            return primaryKeys;
        }

        public async Task<List<KeyMetadata>> GetForeignKeysAsync(string tableName)
        {
            var foreignKeys = new List<KeyMetadata>();
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
            return foreignKeys;
        }
        
        public async Task<Dictionary<string, List<string>>> GetForeignKeyRelationshipsAsync()
        {
            var relationships = new Dictionary<string, List<string>>();
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
            return relationships;
        }
    }
}
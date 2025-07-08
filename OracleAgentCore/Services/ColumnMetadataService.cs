using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using OracleAgent.Core.Models;
using OracleAgent.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace OracleAgent.Core.Services
{

    public class ColumnMetadataService : IColumnMetadataService
    {
        private readonly string _connectionString;

        public ColumnMetadataService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<List<ColumnMetadata>> GetColumnMetadataAsync(string tableName)
        {
            var columnMetadataList = new List<ColumnMetadata>();
            using (var connection = new OracleConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = $@"
                    SELECT COLUMN_NAME, DATA_TYPE, NULLABLE, DATA_DEFAULT
                    FROM ALL_TAB_COLUMNS
                    WHERE TABLE_NAME = :TableName";

                using (var command = new OracleCommand(query, connection))
                {
                    command.Parameters.Add(new OracleParameter("TableName", tableName.ToUpper()));

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            columnMetadataList.Add(new ColumnMetadata
                            {
                                Name = reader["COLUMN_NAME"].ToString(),
                                DataType = reader["DATA_TYPE"].ToString(),
                                IsNullable = reader["NULLABLE"].ToString() == "Y",
                                DefaultValue = reader["DATA_DEFAULT"]?.ToString()
                            });
                        }
                    }
                }
            }

            return columnMetadataList;
        }

        public async Task<List<string>> GetColumnNamesAsync(string tableName)
        {
            var columnNames = new List<string>();
            using (var connection = new OracleConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"
                    SELECT COLUMN_NAME
                    FROM ALL_TAB_COLUMNS
                    WHERE TABLE_NAME = :TableName";

                using (var command = new OracleCommand(query, connection))
                {
                    command.Parameters.Add(new OracleParameter("TableName", tableName.ToUpper()));

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            columnNames.Add(reader["COLUMN_NAME"].ToString());
                        }
                    }
                }
            }
            return columnNames;
        }

        public async Task<List<ColumnMetadata>> GetDataTypesAsync(string tableName)
        {
            var columnMetadataList = new List<ColumnMetadata>();
            using (var connection = new OracleConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"
                    SELECT COLUMN_NAME, DATA_TYPE
                    FROM ALL_TAB_COLUMNS
                    WHERE TABLE_NAME = :TableName";

                using (var command = new OracleCommand(query, connection))
                {
                    command.Parameters.Add(new OracleParameter("TableName", tableName.ToUpper()));

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            columnMetadataList.Add(new ColumnMetadata
                            {
                                Name = reader["COLUMN_NAME"].ToString(),
                                DataType = reader["DATA_TYPE"].ToString()
                            });
                        }
                    }
                }
            }

            return columnMetadataList;
        }

        public async Task<List<ColumnMetadata>> GetNullabilityAsync(string tableName)
        {
            var columnMetadataList = new List<ColumnMetadata>();
            using (var connection = new OracleConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"
                    SELECT COLUMN_NAME, NULLABLE
                    FROM ALL_TAB_COLUMNS
                    WHERE TABLE_NAME = :TableName";

                using (var command = new OracleCommand(query, connection))
                {
                    command.Parameters.Add(new OracleParameter("TableName", tableName.ToUpper()));

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            columnMetadataList.Add(new ColumnMetadata
                            {
                                Name = reader["COLUMN_NAME"].ToString(),
                                IsNullable = reader["NULLABLE"].ToString() == "Y"
                            });
                        }
                    }
                }
            }

            return columnMetadataList;
        }

        public async Task<List<ColumnMetadata>> GetDefaultValuesAsync(string tableName)
        {
            var columnMetadataList = new List<ColumnMetadata>();
            using (var connection = new OracleConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"
                    SELECT COLUMN_NAME, DATA_DEFAULT
                    FROM ALL_TAB_COLUMNS
                    WHERE TABLE_NAME = :TableName";

                using (var command = new OracleCommand(query, connection))
                {
                    command.Parameters.Add(new OracleParameter("TableName", tableName.ToUpper()));

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            columnMetadataList.Add(new ColumnMetadata
                            {
                                Name = reader["COLUMN_NAME"].ToString(),
                                DefaultValue = reader["DATA_DEFAULT"]?.ToString()
                            });
                        }
                    }
                }
            }

            return columnMetadataList;
        }
        
        public async Task<List<string>> GetTablesByColumnNameAsync(string columnNamePattern)
        {
            var tableNames = new List<string>();
            using (var connection = new OracleConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"
                    SELECT DISTINCT TABLE_NAME
                    FROM USER_TAB_COLUMNS
                    WHERE UPPER(COLUMN_NAME) LIKE :ColumnNamePattern";

                using (var command = new OracleCommand(query, connection))
                {
                    command.Parameters.Add(new OracleParameter("ColumnNamePattern", $"%{columnNamePattern.ToUpper()}%"));

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            tableNames.Add(reader["TABLE_NAME"].ToString());
                        }
                    }
                }
            }
            return tableNames;
        }
    }
}
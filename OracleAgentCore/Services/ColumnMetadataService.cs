using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OracleAgent.Core.Models;
using OracleAgent.Core.Interfaces;

namespace OracleAgent.Core.Services
{
    public class ColumnMetadataService : IColumnMetadataService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<ColumnMetadataService> _logger;

        public ColumnMetadataService(IDbConnectionFactory connectionFactory, ILogger<ColumnMetadataService> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<ColumnMetadata>> GetColumnMetadataAsync(string tableName)
        {
            _logger.LogInformation("Getting column metadata for table: {TableName}", tableName);
            var columnMetadataList = new List<ColumnMetadata>();
            try
            {
                using (var connection = await _connectionFactory.CreateConnectionAsync())
                {
                    var query = @"
                        SELECT COLUMN_NAME, DATA_TYPE, NULLABLE, DATA_DEFAULT
                        FROM ALL_TAB_COLUMNS
                        WHERE TABLE_NAME = :TableName";

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = query;
                        var param = command.CreateParameter();
                        param.ParameterName = "TableName";
                        param.Value = tableName?.ToUpper();
                        command.Parameters.Add(param);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
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
                _logger.LogInformation("Retrieved {Count} columns for table {TableName}", columnMetadataList.Count, tableName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting column metadata for table: {TableName}", tableName);
                throw;
            }
            return columnMetadataList;
        }

        public async Task<List<string>> GetColumnNamesAsync(string tableName)
        {
            _logger.LogInformation("Getting column names for table: {TableName}", tableName);
            var columnNames = new List<string>();
            try
            {
                using (var connection = await _connectionFactory.CreateConnectionAsync())
                {
                    var query = @"
                        SELECT COLUMN_NAME
                        FROM ALL_TAB_COLUMNS
                        WHERE TABLE_NAME = :TableName";

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = query;
                        var param = command.CreateParameter();
                        param.ParameterName = "TableName";
                        param.Value = tableName?.ToUpper();
                        command.Parameters.Add(param);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                columnNames.Add(reader["COLUMN_NAME"].ToString());
                            }
                        }
                    }
                }
                _logger.LogInformation("Retrieved {Count} column names for table {TableName}", columnNames.Count, tableName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting column names for table: {TableName}", tableName);
                throw;
            }
            return columnNames;
        }

        public async Task<List<ColumnMetadata>> GetDataTypesAsync(string tableName)
        {
            _logger.LogInformation("Getting data types for table: {TableName}", tableName);
            var columnMetadataList = new List<ColumnMetadata>();
            try
            {
                using (var connection = await _connectionFactory.CreateConnectionAsync())
                {
                    var query = @"
                        SELECT COLUMN_NAME, DATA_TYPE
                        FROM ALL_TAB_COLUMNS
                        WHERE TABLE_NAME = :TableName";

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = query;
                        var param = command.CreateParameter();
                        param.ParameterName = "TableName";
                        param.Value = tableName?.ToUpper();
                        command.Parameters.Add(param);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
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
                _logger.LogInformation("Retrieved {Count} data types for table {TableName}", columnMetadataList.Count, tableName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting data types for table: {TableName}", tableName);
                throw;
            }
            return columnMetadataList;
        }

        public async Task<List<ColumnMetadata>> GetNullabilityAsync(string tableName)
        {
            _logger.LogInformation("Getting nullability for table: {TableName}", tableName);
            var columnMetadataList = new List<ColumnMetadata>();
            try
            {
                using (var connection = await _connectionFactory.CreateConnectionAsync())
                {
                    var query = @"
                        SELECT COLUMN_NAME, NULLABLE
                        FROM ALL_TAB_COLUMNS
                        WHERE TABLE_NAME = :TableName";

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = query;
                        var param = command.CreateParameter();
                        param.ParameterName = "TableName";
                        param.Value = tableName?.ToUpper();
                        command.Parameters.Add(param);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
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
                _logger.LogInformation("Retrieved {Count} nullability columns for table {TableName}", columnMetadataList.Count, tableName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting nullability for table: {TableName}", tableName);
                throw;
            }
            return columnMetadataList;
        }

        public async Task<List<ColumnMetadata>> GetDefaultValuesAsync(string tableName)
        {
            _logger.LogInformation("Getting default values for table: {TableName}", tableName);
            var columnMetadataList = new List<ColumnMetadata>();
            try
            {
                using (var connection = await _connectionFactory.CreateConnectionAsync())
                {
                    var query = @"
                        SELECT COLUMN_NAME, DATA_DEFAULT
                        FROM ALL_TAB_COLUMNS
                        WHERE TABLE_NAME = :TableName";

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = query;
                        var param = command.CreateParameter();
                        param.ParameterName = "TableName";
                        param.Value = tableName?.ToUpper();
                        command.Parameters.Add(param);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
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
                _logger.LogInformation("Retrieved {Count} default values for table {TableName}", columnMetadataList.Count, tableName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default values for table: {TableName}", tableName);
                throw;
            }
            return columnMetadataList;
        }

        public async Task<List<string>> GetTablesByColumnNameAsync(string columnNamePattern)
        {
            _logger.LogInformation("Getting tables by column name pattern: {ColumnNamePattern}", columnNamePattern);
            var tableNames = new List<string>();
            try
            {
                using (var connection = await _connectionFactory.CreateConnectionAsync())
                {
                    var query = @"
                        SELECT DISTINCT TABLE_NAME
                        FROM USER_TAB_COLUMNS
                        WHERE UPPER(COLUMN_NAME) LIKE :ColumnNamePattern";

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = query;
                        var param = command.CreateParameter();
                        param.ParameterName = "ColumnNamePattern";
                        param.Value = $"%{columnNamePattern?.ToUpper()}%";
                        command.Parameters.Add(param);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tableNames.Add(reader["TABLE_NAME"].ToString());
                            }
                        }
                    }
                }
                _logger.LogInformation("Retrieved {Count} tables for column name pattern {ColumnNamePattern}", tableNames.Count, columnNamePattern);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tables by column name pattern: {ColumnNamePattern}", columnNamePattern);
                throw;
            }
            return tableNames;
        }
    }
}
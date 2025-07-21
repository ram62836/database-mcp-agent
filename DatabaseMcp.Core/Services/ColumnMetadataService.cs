using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DatabaseMcp.Core.Interfaces;
using DatabaseMcp.Core.Models;
using Microsoft.Extensions.Configuration;

namespace DatabaseMcp.Core.Services
{
    public class ColumnMetadataService : IColumnMetadataService
    {
        private readonly string _metadataJsonDirectory;
    
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<ColumnMetadataService> _logger;

        public ColumnMetadataService(IDbConnectionFactory connectionFactory, IConfiguration config, ILogger<ColumnMetadataService> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _metadataJsonDirectory = config["MetadataJsonPath"] ?? AppConstants.ExecutableDirectory;
        }

        // Example usage of _metadataJsonDirectory for reading/writing metadata JSON files
        private string GetMetadataFilePath(string tableName)
        {
            return Path.Combine(_metadataJsonDirectory, $"{tableName}_metadata.json");
        }

        public async Task<List<ColumnMetadata>> GetColumnMetadataAsync(string tableName)
        {
            _logger.LogInformation("Getting column metadata for table: {TableName}", tableName);
            List<ColumnMetadata> columnMetadataList = new();
            string metadataFilePath = GetMetadataFilePath(tableName);
            // Example: Check if metadata file exists and read from it
            if (File.Exists(metadataFilePath))
            {
                try
                {
                    string json = await File.ReadAllTextAsync(metadataFilePath);
                    var cachedMetadata = System.Text.Json.JsonSerializer.Deserialize<List<ColumnMetadata>>(json);
                    if (cachedMetadata != null)
                    {
                        _logger.LogInformation("Loaded column metadata from JSON file: {FilePath}", metadataFilePath);
                        return cachedMetadata;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to read metadata JSON file: {FilePath}", metadataFilePath);
                }
            }
            try
            {
                using (System.Data.IDbConnection connection = await _connectionFactory.CreateConnectionAsync())
                {
                    string query = @"
                        SELECT COLUMN_NAME, DATA_TYPE, NULLABLE, DATA_DEFAULT
                        FROM ALL_TAB_COLUMNS
                        WHERE TABLE_NAME = :TableName";

                    using System.Data.IDbCommand command = connection.CreateCommand();
                    command.CommandText = query;
                    System.Data.IDbDataParameter param = command.CreateParameter();
                    param.ParameterName = "TableName";
                    param.Value = tableName?.ToUpper();
                    _ = command.Parameters.Add(param);

                    using System.Data.IDataReader reader = command.ExecuteReader();
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
                _logger.LogInformation("Retrieved {Count} columns for table {TableName}", columnMetadataList.Count, tableName);
                // Example: Write metadata to JSON file for caching
                try
                {
                    Directory.CreateDirectory(_metadataJsonDirectory);
                    string json = System.Text.Json.JsonSerializer.Serialize(columnMetadataList);
                    await File.WriteAllTextAsync(metadataFilePath, json);
                    _logger.LogInformation("Saved column metadata to JSON file: {FilePath}", metadataFilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to write metadata JSON file: {FilePath}", metadataFilePath);
                }
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
            List<string> columnNames = new();
            try
            {
                using (System.Data.IDbConnection connection = await _connectionFactory.CreateConnectionAsync())
                {
                    string query = @"
                        SELECT COLUMN_NAME
                        FROM ALL_TAB_COLUMNS
                        WHERE TABLE_NAME = :TableName";

                    using System.Data.IDbCommand command = connection.CreateCommand();
                    command.CommandText = query;
                    System.Data.IDbDataParameter param = command.CreateParameter();
                    param.ParameterName = "TableName";
                    param.Value = tableName?.ToUpper();
                    _ = command.Parameters.Add(param);

                    using System.Data.IDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        columnNames.Add(reader["COLUMN_NAME"].ToString());
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
            List<ColumnMetadata> columnMetadataList = new();
            try
            {
                using (System.Data.IDbConnection connection = await _connectionFactory.CreateConnectionAsync())
                {
                    string query = @"
                        SELECT COLUMN_NAME, DATA_TYPE
                        FROM ALL_TAB_COLUMNS
                        WHERE TABLE_NAME = :TableName";

                    using System.Data.IDbCommand command = connection.CreateCommand();
                    command.CommandText = query;
                    System.Data.IDbDataParameter param = command.CreateParameter();
                    param.ParameterName = "TableName";
                    param.Value = tableName?.ToUpper();
                    _ = command.Parameters.Add(param);

                    using System.Data.IDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        columnMetadataList.Add(new ColumnMetadata
                        {
                            Name = reader["COLUMN_NAME"].ToString(),
                            DataType = reader["DATA_TYPE"].ToString()
                        });
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
            List<ColumnMetadata> columnMetadataList = new();
            try
            {
                using (System.Data.IDbConnection connection = await _connectionFactory.CreateConnectionAsync())
                {
                    string query = @"
                        SELECT COLUMN_NAME, NULLABLE
                        FROM ALL_TAB_COLUMNS
                        WHERE TABLE_NAME = :TableName";

                    using System.Data.IDbCommand command = connection.CreateCommand();
                    command.CommandText = query;
                    System.Data.IDbDataParameter param = command.CreateParameter();
                    param.ParameterName = "TableName";
                    param.Value = tableName?.ToUpper();
                    _ = command.Parameters.Add(param);

                    using System.Data.IDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        columnMetadataList.Add(new ColumnMetadata
                        {
                            Name = reader["COLUMN_NAME"].ToString(),
                            IsNullable = reader["NULLABLE"].ToString() == "Y"
                        });
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
            List<ColumnMetadata> columnMetadataList = new();
            try
            {
                using (System.Data.IDbConnection connection = await _connectionFactory.CreateConnectionAsync())
                {
                    string query = @"
                        SELECT COLUMN_NAME, DATA_DEFAULT
                        FROM ALL_TAB_COLUMNS
                        WHERE TABLE_NAME = :TableName";

                    using System.Data.IDbCommand command = connection.CreateCommand();
                    command.CommandText = query;
                    System.Data.IDbDataParameter param = command.CreateParameter();
                    param.ParameterName = "TableName";
                    param.Value = tableName?.ToUpper();
                    _ = command.Parameters.Add(param);

                    using System.Data.IDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        columnMetadataList.Add(new ColumnMetadata
                        {
                            Name = reader["COLUMN_NAME"].ToString(),
                            DefaultValue = reader["DATA_DEFAULT"]?.ToString()
                        });
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
            List<string> tableNames = new();
            try
            {
                using (System.Data.IDbConnection connection = await _connectionFactory.CreateConnectionAsync())
                {
                    string query = @"
                        SELECT DISTINCT TABLE_NAME
                        FROM USER_TAB_COLUMNS
                        WHERE UPPER(COLUMN_NAME) LIKE :ColumnNamePattern";

                    using System.Data.IDbCommand command = connection.CreateCommand();
                    command.CommandText = query;
                    System.Data.IDbDataParameter param = command.CreateParameter();
                    param.ParameterName = "ColumnNamePattern";
                    param.Value = $"%{columnNamePattern?.ToUpper()}%";
                    _ = command.Parameters.Add(param);

                    using System.Data.IDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        tableNames.Add(reader["TABLE_NAME"].ToString());
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
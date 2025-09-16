using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using DatabaseMcp.Core.Interfaces;
using DatabaseMcp.Core.Models;
using Microsoft.Extensions.Logging;

namespace DatabaseMcp.Core.Services
{
    public class IndexListingService : IIndexListingService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<IndexListingService> _logger;
        private readonly string _owner;

        public IndexListingService(IDbConnectionFactory connectionFactory, ILogger<IndexListingService> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger;
            _owner = Environment.GetEnvironmentVariable("SchemaOwner");
        }

        public async Task<List<IndexMetadata>> ListIndexesAsync(string tableName)
        {
            _logger.LogInformation("Listing indexes for table: {TableName}", tableName);
            List<IndexMetadata> indexes = [];
            try
            {
                using (IDbConnection connection = await _connectionFactory.CreateConnectionAsync())
                {
                    string query = @"SELECT INDEX_NAME, UNIQUENESS FROM ALL_INDEXES WHERE TABLE_NAME = :TableName AND OWNER = :Owner";

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
                        indexes.Add(new IndexMetadata
                        {
                            IndexName = reader["INDEX_NAME"].ToString(),
                            IsUnique = reader["UNIQUENESS"].ToString() == "UNIQUE"
                        });
                    }
                }
                _logger.LogInformation("Retrieved {Count} indexes for table {TableName}", indexes.Count, tableName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing indexes for table: {TableName}", tableName);
                throw;
            }
            return indexes;
        }

        public async Task<List<string>> GetIndexColumnsAsync(string indexName)
        {
            _logger.LogInformation("Getting index columns for index: {IndexName}", indexName);
            List<string> columns = [];
            try
            {
                using (IDbConnection connection = await _connectionFactory.CreateConnectionAsync())
                {
                    string query = @"SELECT COLUMN_NAME FROM ALL_IND_COLUMNS WHERE INDEX_NAME = :IndexName AND INDEX_OWNER = :Owner";

                    using IDbCommand command = connection.CreateCommand();
                    command.CommandText = query;
                    IDbDataParameter param = command.CreateParameter();
                    param.ParameterName = "IndexName";
                    param.Value = indexName.ToUpper();
                    _ = command.Parameters.Add(param);
                    IDbDataParameter ownerParam = command.CreateParameter();
                    ownerParam.ParameterName = "Owner";
                    ownerParam.Value = _owner;
                    _ = command.Parameters.Add(ownerParam);

                    using IDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        columns.Add(reader["COLUMN_NAME"].ToString());
                    }
                }
                _logger.LogInformation("Retrieved {Count} columns for index {IndexName}", columns.Count, indexName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting index columns for index: {IndexName}", indexName);
                throw;
            }
            return columns;
        }
    }
}
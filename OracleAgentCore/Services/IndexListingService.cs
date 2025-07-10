using System;
using System.Collections.Generic;
using OracleAgent.Core.Models;
using OracleAgent.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Data;

namespace OracleAgent.Core.Services
{
    public class IndexListingService : IIndexListingService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<IndexListingService> _logger;

        public IndexListingService(IDbConnectionFactory connectionFactory, ILogger<IndexListingService> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger;
        }

        public async Task<List<IndexMetadata>> ListIndexesAsync(string tableName)
        {
            _logger.LogInformation("Listing indexes for table: {TableName}", tableName);
            var indexes = new List<IndexMetadata>();
            try
            {
                using (var connection = await _connectionFactory.CreateConnectionAsync())
                {
                    var query = @"SELECT INDEX_NAME, UNIQUENESS FROM ALL_INDEXES WHERE TABLE_NAME = :TableName";

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
                                indexes.Add(new IndexMetadata
                                {
                                    IndexName = reader["INDEX_NAME"].ToString(),
                                    IsUnique = reader["UNIQUENESS"].ToString() == "UNIQUE"
                                });
                            }
                        }
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
            var columns = new List<string>();
            try
            {
                using (var connection = await _connectionFactory.CreateConnectionAsync())
                {
                    var query = @"SELECT COLUMN_NAME FROM ALL_IND_COLUMNS WHERE INDEX_NAME = :IndexName";

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = query;
                        var param = command.CreateParameter();
                        param.ParameterName = "IndexName";
                        param.Value = indexName.ToUpper();
                        command.Parameters.Add(param);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                columns.Add(reader["COLUMN_NAME"].ToString());
                            }
                        }
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
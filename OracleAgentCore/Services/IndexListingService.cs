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
    public class IndexListingService : IIndexListingService
    {
        private readonly string _connectionString;
        private readonly ILogger<IndexListingService> _logger;

        public IndexListingService(IConfiguration config, ILogger<IndexListingService> logger)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public async Task<List<IndexMetadata>> ListIndexesAsync(string tableName)
        {
            _logger.LogInformation("Listing indexes for table: {TableName}", tableName);
            var indexes = new List<IndexMetadata>();
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = @"SELECT INDEX_NAME, UNIQUENESS  
                                  FROM ALL_INDEXES  
                                  WHERE TABLE_NAME = :TableName";

                    using (var command = new OracleCommand(query, connection))
                    {
                        command.Parameters.Add(new OracleParameter("TableName", tableName.ToUpper()));

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
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
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = @"SELECT COLUMN_NAME  
                                  FROM ALL_IND_COLUMNS  
                                  WHERE INDEX_NAME = :IndexName";

                    using (var command = new OracleCommand(query, connection))
                    {
                        command.Parameters.Add(new OracleParameter("IndexName", indexName.ToUpper()));

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
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
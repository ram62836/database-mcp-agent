using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using OracleAgent.Core.Models;
using OracleAgent.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace OracleAgent.Core.Services
{
    public class IndexListingService : IIndexListingService
    {
        private readonly string _connectionString;

        public IndexListingService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<List<IndexMetadata>> ListIndexesAsync(string tableName)
        {
            var indexes = new List<IndexMetadata>();
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
            return indexes;
        }

        public async Task<List<string>> GetIndexColumnsAsync(string indexName)
        {
            var columns = new List<string>();
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
            return columns;
        }
    }
}
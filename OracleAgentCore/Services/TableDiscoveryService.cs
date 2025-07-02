using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using OracleAgent.Core.Models;
using OracleAgent.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Text.Json;
using System.IO;

namespace OracleAgent.Core.Services
{
    public class TableDiscoveryService : ITableDiscoveryService
    {
        private readonly string _connectionString;        

        public TableDiscoveryService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<List<TableMetadata>> GetAllUserDefinedTablesAsync()
        {
            if (File.Exists(AppConstants.TablesMetadatJsonFile))
            {
                var fileContent = await File.ReadAllTextAsync(AppConstants.TablesMetadatJsonFile);
                List<TableMetadata> cachedTableMetadata = JsonSerializer.Deserialize<List<TableMetadata>>(fileContent);
                return cachedTableMetadata;
            }

            var tablesMetadata = new List<TableMetadata>();
            using (var connection = new OracleConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"SELECT TABLE_NAME, DBMS_METADATA.GET_DDL('TABLE', TABLE_NAME) AS TABLE_DDL
                                  FROM USER_TABLES
                                  WHERE TEMPORARY = 'N' AND NESTED = 'NO' AND SECONDARY = 'N' AND TABLE_NAME NOT LIKE 'SYS\_%' ESCAPE '\'";

                using (var command = new OracleCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            tablesMetadata.Add(new TableMetadata
                            {
                                TableName = reader["TABLE_NAME"].ToString(),
                                Definition = reader["TABLE_DDL"].ToString()
                            });
                        }
                    }
                }
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(tablesMetadata, options);
            await File.WriteAllTextAsync(AppConstants.TablesMetadatJsonFile, json);
            return tablesMetadata;
        }

        public async Task<List<TableMetadata>> GetTablesByNameAsync(List<string> tableNames)
        {
            var tablesMetadata = await GetAllUserDefinedTablesAsync();
            var filteredTables = tablesMetadata
                .Where(table => tableNames.Any(name => table.TableName.Contains(name, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            return filteredTables;
        }

        private async Task<string> GetTableDefinitionAsync(string tableName)
        {
            using (var connection = new OracleConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new OracleCommand(@"SELECT DBMS_METADATA.GET_DDL('TABLE', :TableName) AS DDL FROM DUAL", connection);
                command.Parameters.Add(new OracleParameter("TableName", tableName));

                return (await command.ExecuteScalarAsync())?.ToString() ?? string.Empty;
            }
        }
    }
}
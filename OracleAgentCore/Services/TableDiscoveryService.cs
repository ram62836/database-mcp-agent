using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using OracleAgent.Core.Models;
using OracleAgent.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<TableDiscoveryService> _logger;

        public TableDiscoveryService(IConfiguration config, ILogger<TableDiscoveryService> logger)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public async Task<List<TableMetadata>> GetAllUserDefinedTablesAsync()
        {
            _logger.LogInformation("Getting all user-defined tables.");
            if (File.Exists(AppConstants.TablesMetadatJsonFile))
            {
                var fileContent = await File.ReadAllTextAsync(AppConstants.TablesMetadatJsonFile);
                List<TableMetadata> cachedTableMetadata = JsonSerializer.Deserialize<List<TableMetadata>>(fileContent);
                _logger.LogInformation("Loaded {Count} tables from cache.", cachedTableMetadata?.Count ?? 0);
                return cachedTableMetadata;
            }

            var tablesMetadata = new List<TableMetadata>();
            try
            {
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
                _logger.LogInformation("Retrieved {Count} user-defined tables.", tablesMetadata.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user-defined tables.");
                throw;
            }
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(tablesMetadata, options);
            await File.WriteAllTextAsync(AppConstants.TablesMetadatJsonFile, json);
            return tablesMetadata;
        }

        public async Task<List<TableMetadata>> GetTablesByNameAsync(List<string> tableNames)
        {
            _logger.LogInformation("Getting tables by name.");
            var tablesMetadata = await GetAllUserDefinedTablesAsync();
            var filteredTables = tablesMetadata
                .Where(table => tableNames.Any(name => table.TableName.Contains(name, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            _logger.LogInformation("Filtered to {Count} tables by name.", filteredTables.Count);
            return filteredTables;
        }

        private async Task<string> GetTableDefinitionAsync(string tableName)
        {
            _logger.LogInformation("Getting table definition for: {TableName}", tableName);
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var command = new OracleCommand(@"SELECT DBMS_METADATA.GET_DDL('TABLE', :TableName) AS DDL FROM DUAL", connection);
                    command.Parameters.Add(new OracleParameter("TableName", tableName));

                    return (await command.ExecuteScalarAsync())?.ToString() ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting table definition for: {TableName}", tableName);
                throw;
            }
        }
    }
}
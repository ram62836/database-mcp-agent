using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DatabaseMcp.Core.Interfaces;
using DatabaseMcp.Core.Models;
using Microsoft.Extensions.Configuration;

namespace DatabaseMcp.Core.Services
{
    public class TableDiscoveryService : ITableDiscoveryService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<TableDiscoveryService> _logger;

        public TableDiscoveryService(IDbConnectionFactory connectionFactory, ILogger<TableDiscoveryService> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger;
        }

        public async Task<List<TableMetadata>> GetAllUserDefinedTablesAsync()
        {
            _logger.LogInformation("Getting all user-defined tables.");
            if (File.Exists(AppConstants.TablesMetadatJsonFile))
            {
                try 
                {
                    string fileContent = await File.ReadAllTextAsync(AppConstants.TablesMetadatJsonFile);
                    var deserializeOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    List<TableMetadata> cachedTableMetadata = JsonSerializer.Deserialize<List<TableMetadata>>(fileContent, deserializeOptions);
                    
                    // Ensure we don't return null
                    if (cachedTableMetadata == null)
                    {
                        _logger.LogWarning("Cache file deserialized to null, returning empty list.");
                        return new List<TableMetadata>();
                    }
                    
                    _logger.LogInformation("Loaded {0} tables from cache.", cachedTableMetadata.Count);
                    return cachedTableMetadata;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading table metadata from cache file. Proceeding with database query.");
                    // Continue to database query path
                }
            }

            List<TableMetadata> tablesMetadata = new();
            try
            {
                using (IDbConnection connection = await _connectionFactory.CreateConnectionAsync())
                {
                    string query = @"SELECT TABLE_NAME, DBMS_METADATA.GET_DDL('TABLE', TABLE_NAME) AS TABLE_DDL FROM USER_TABLES WHERE TEMPORARY = 'N' AND NESTED = 'NO' AND SECONDARY = 'N' AND TABLE_NAME NOT LIKE 'SYS\_%' ESCAPE '\'";

                    using IDbCommand command = connection.CreateCommand();
                    command.CommandText = query;
                    using IDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        tablesMetadata.Add(new TableMetadata
                        {
                            TableName = reader["TABLE_NAME"].ToString(),
                            Definition = reader["TABLE_DDL"].ToString()
                        });
                    }
                }
                _logger.LogInformation("Retrieved {Count} user-defined tables.", tablesMetadata.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user-defined tables.");
                throw;
            }
            JsonSerializerOptions options = new() { WriteIndented = true };
            string json = JsonSerializer.Serialize(tablesMetadata, options);
            Directory.CreateDirectory(AppConstants.ExecutableDirectory);
            await File.WriteAllTextAsync(AppConstants.TablesMetadatJsonFile, json);
            return tablesMetadata;
        }

        public async Task<List<TableMetadata>> GetTablesByNameAsync(List<string> tableNames)
        {
            _logger.LogInformation("Getting tables by name.");
            
            // Handle null or empty table names list
            if (tableNames == null || tableNames.Count == 0)
            {
                _logger.LogInformation("No table names provided, returning empty list.");
                return new List<TableMetadata>();
            }
            
            // Get all tables
            List<TableMetadata> tablesMetadata = await GetAllUserDefinedTablesAsync();
            
            // Handle null tablesMetadata (shouldn't happen with our fixes, but just in case)
            if (tablesMetadata == null)
            {
                _logger.LogWarning("Tables metadata was null, returning empty list.");
                return new List<TableMetadata>();
            }
            
            // Filter tables safely
            List<TableMetadata> filteredTables = tablesMetadata
                .Where(table => 
                    table != null && 
                    table.TableName != null && 
                    tableNames.Any(name => 
                        name != null && table.TableName.Contains(name, StringComparison.OrdinalIgnoreCase)))
                .ToList();
                
            _logger.LogInformation("Filtered to {0} tables by name.", filteredTables.Count);
            return filteredTables;
        }
    }
}
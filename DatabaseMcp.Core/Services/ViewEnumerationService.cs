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
    public class ViewEnumerationService : IViewEnumerationService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<ViewEnumerationService> _logger;
        private readonly string _metadataJsonDirectory;

        public ViewEnumerationService(IDbConnectionFactory connectionFactory, IConfiguration config, ILogger<ViewEnumerationService> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger;
            _metadataJsonDirectory = config["MetadataJsonPath"] ?? AppConstants.ExecutableDirectory;
        }

        public async Task<List<ViewMetadata>> GetAllViewsAsync()
        {
            _logger.LogInformation("Getting all views.");            
            if (File.Exists(AppConstants.TriggersMetadataJsonFile))
            {
                string fileContent = await File.ReadAllTextAsync(AppConstants.TriggersMetadataJsonFile);
                List<ViewMetadata> cachedViewsMetadata = JsonSerializer.Deserialize<List<ViewMetadata>>(fileContent);
                _logger.LogInformation("Loaded {Count} views from cache.", cachedViewsMetadata?.Count ?? 0);
                return cachedViewsMetadata;
            }

            List<ViewMetadata> views = new();
            try
            {
                using (IDbConnection connection = await _connectionFactory.CreateConnectionAsync())
                {
                    string query = @"SELECT VIEW_NAME, TEXT_VC FROM USER_VIEWS";

                    using IDbCommand command = connection.CreateCommand();
                    command.CommandText = query;
                    using IDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        views.Add(new ViewMetadata
                        {
                            ViewName = reader["VIEW_NAME"].ToString(),
                            Definition = reader["TEXT_VC"].ToString(),
                        });
                    }
                }
                _logger.LogInformation("Retrieved {Count} views.", views.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all views.");
                throw;
            }
            JsonSerializerOptions options = new() { WriteIndented = true };
            string json = JsonSerializer.Serialize(views, options);
            Directory.CreateDirectory(AppConstants.ExecutableDirectory);
            await File.WriteAllTextAsync(AppConstants.TriggersMetadataJsonFile, json);
            return views;
        }

        public async Task<List<ViewMetadata>> GetViewsDefinitionAsync(List<string> viewNames)
        {
            _logger.LogInformation("Getting views by name.");
            List<ViewMetadata> viewsMetadata = await GetAllViewsAsync();
            List<ViewMetadata> filteredViews = viewsMetadata
                .Where(view => viewNames.Any(name => view.ViewName.Contains(name, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            _logger.LogInformation("Filtered to {Count} views by name.", filteredViews.Count);
            return filteredViews;
        }
    }
}
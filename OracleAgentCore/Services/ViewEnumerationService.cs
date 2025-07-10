using System;
using System.Collections.Generic;
using OracleAgent.Core.Models;
using OracleAgent.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using System.Linq;
using System.Data;

namespace OracleAgent.Core.Services
{
    public class ViewEnumerationService : IViewEnumerationService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<ViewEnumerationService> _logger;

        public ViewEnumerationService(IDbConnectionFactory connectionFactory, ILogger<ViewEnumerationService> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger;
        }

        public async Task<List<ViewMetadata>> GetAllViewsAsync()
        {
            _logger.LogInformation("Getting all views.");
            if (File.Exists(AppConstants.ViewsMetadatJsonFile))
            {
                var fileContent = await File.ReadAllTextAsync(AppConstants.ViewsMetadatJsonFile);
                List<ViewMetadata> cachedViewsMetadata = JsonSerializer.Deserialize<List<ViewMetadata>>(fileContent);
                _logger.LogInformation("Loaded {Count} views from cache.", cachedViewsMetadata?.Count ?? 0);
                return cachedViewsMetadata;
            }

            var views = new List<ViewMetadata>();
            try
            {
                using (var connection = await _connectionFactory.CreateConnectionAsync())
                {
                    var query = @"SELECT VIEW_NAME, TEXT_VC FROM USER_VIEWS";

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = query;
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {                            
                                views.Add(new ViewMetadata
                                {
                                    ViewName = reader["VIEW_NAME"].ToString(),
                                    Definition = reader["TEXT_VC"].ToString(),
                                });
                            }
                        }
                    }
                }
                _logger.LogInformation("Retrieved {Count} views.", views.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all views.");
                throw;
            }
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(views, options);
            await File.WriteAllTextAsync(AppConstants.ViewsMetadatJsonFile, json);
            return views;
        }

        public async Task<List<ViewMetadata>> GetViewsDefinitionAsync(List<string> viewNames)
        {
            _logger.LogInformation("Getting views by name.");
            var viewsMetadata = await GetAllViewsAsync();
            var filteredViews = viewsMetadata
                .Where(view => viewNames.Any(name => view.ViewName.Contains(name, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            _logger.LogInformation("Filtered to {Count} views by name.", filteredViews.Count);
            return filteredViews;
        }
    }
}
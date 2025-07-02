using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using OracleAgent.Core.Models;
using OracleAgent.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using System.Linq;

namespace OracleAgent.Core.Services
{
    public class ViewEnumerationService : IViewEnumerationService
    {
        private readonly string _connectionString;        

        public ViewEnumerationService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<List<ViewMetadata>> GetAllViewsAsync()
        {
            if (File.Exists(AppConstants.ViewsMetadatJsonFile))
            {
                var fileContent = await File.ReadAllTextAsync(AppConstants.ViewsMetadatJsonFile);
                List<ViewMetadata> cachedViewsMetadata = JsonSerializer.Deserialize<List<ViewMetadata>>(fileContent);
                return cachedViewsMetadata;
            }

            var views = new List<ViewMetadata>();
            using (var connection = new OracleConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"SELECT VIEW_NAME, TEXT_VC FROM USER_VIEWS";

                using (var command = new OracleCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
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

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(views, options);
            await File.WriteAllTextAsync(AppConstants.ViewsMetadatJsonFile, json);
            return views;
        }

        public async Task<List<ViewMetadata>> GetViewsDefinitionAsync(List<string> viewNames)
        {
            var viewsMetadata = await GetAllViewsAsync();
            var filteredViews = viewsMetadata
                .Where(view => viewNames.Any(name => view.ViewName.Contains(name, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            return filteredViews;
        }
    }
}
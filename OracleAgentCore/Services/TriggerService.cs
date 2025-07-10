using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;
using OracleAgent.Core.Models;
using OracleAgent.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Data;

namespace OracleAgent.Core.Services
{
    public class TriggerService : ITriggerService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<TriggerService> _logger;

        public TriggerService(IDbConnectionFactory connectionFactory, ILogger<TriggerService> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger;
        }

        public async Task<List<TriggerMetadata>> GetAllTriggersAsync()
        {
            _logger.LogInformation("Getting all triggers.");
            if (File.Exists(AppConstants.TriggersMetadataJsonFile))
            {
                var fileContent = await File.ReadAllTextAsync(AppConstants.TriggersMetadataJsonFile);
                List<TriggerMetadata> cachedTriggersMetadata = JsonSerializer.Deserialize<List<TriggerMetadata>>(fileContent);
                _logger.LogInformation("Loaded {Count} triggers from cache.", cachedTriggersMetadata?.Count ?? 0);
                return cachedTriggersMetadata;
            }

            var triggers = new List<TriggerMetadata>();
            try
            {
                using (var connection = await _connectionFactory.CreateConnectionAsync())
                {
                    var query = @"SELECT TRIGGER_NAME, TRIGGER_TYPE, TRIGGERING_EVENT, TABLE_NAME, DESCRIPTION FROM USER_TRIGGERS";

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = query;
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                triggers.Add(new TriggerMetadata
                                {
                                    TriggerName = reader["TRIGGER_NAME"].ToString(),
                                    TriggerType = reader["TRIGGER_TYPE"].ToString(),
                                    TriggeringEvent = reader["TRIGGERING_EVENT"].ToString(),
                                    TableName = reader["TABLE_NAME"].ToString(),
                                    Description = reader["DESCRIPTION"]?.ToString()
                                });
                            }
                        }
                    }
                }
                _logger.LogInformation("Retrieved {Count} triggers.", triggers.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all triggers.");
                throw;
            }
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(triggers, options);
            await File.WriteAllTextAsync(AppConstants.TriggersMetadataJsonFile, json);
            return triggers;
        }

        public async Task<List<TriggerMetadata>> GetTriggersByNameAsync(List<string> triggerNames)
        {
            _logger.LogInformation("Getting triggers by name.");
            var triggersMetadata = await GetAllTriggersAsync();
            var filteredTriggers = triggersMetadata
                .Where(trigger => triggerNames.Any(name => trigger.TriggerName.Contains(name, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            _logger.LogInformation("Filtered to {Count} triggers by name.", filteredTriggers.Count);
            return filteredTriggers;
        }
    }
}

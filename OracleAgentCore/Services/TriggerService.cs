using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using OracleAgent.Core.Models;
using OracleAgent.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace OracleAgent.Core.Services
{
    public class TriggerService : ITriggerService
    {
        private readonly string _connectionString;

        public TriggerService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<List<TriggerMetadata>> GetAllTriggersAsync()
        {
            if (File.Exists(AppConstants.TriggersMetadataJsonFile))
            {
                var fileContent = await File.ReadAllTextAsync(AppConstants.TriggersMetadataJsonFile);
                List<TriggerMetadata> cachedTriggersMetadata = JsonSerializer.Deserialize<List<TriggerMetadata>>(fileContent);
                return cachedTriggersMetadata;
            }

            var triggers = new List<TriggerMetadata>();
            using (var connection = new OracleConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"SELECT TRIGGER_NAME, TRIGGER_TYPE, TRIGGERING_EVENT, TABLE_NAME, DESCRIPTION FROM USER_TRIGGERS";

                using (var command = new OracleCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
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

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(triggers, options);
            await File.WriteAllTextAsync(AppConstants.TriggersMetadataJsonFile, json);
            return triggers;
        }

        public async Task<List<TriggerMetadata>> GetTriggersByNameAsync(List<string> triggerNames)
        {
            var triggersMetadata = await GetAllTriggersAsync();
            var filteredTriggers = triggersMetadata
                .Where(trigger => triggerNames.Any(name => trigger.TriggerName.Contains(name, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            return filteredTriggers;
        }
    }
}

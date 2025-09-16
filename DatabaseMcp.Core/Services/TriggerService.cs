using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using DatabaseMcp.Core.Interfaces;
using DatabaseMcp.Core.Models;
using Microsoft.Extensions.Logging;

namespace DatabaseMcp.Core.Services
{
    public class TriggerService : ITriggerService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<TriggerService> _logger;
        private readonly string _owner;

        public TriggerService(IDbConnectionFactory connectionFactory, ILogger<TriggerService> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger;
            _owner = Environment.GetEnvironmentVariable("SchemaOwner");
        }

        public async Task<List<TriggerMetadata>> GetTriggersMetadatByNamesAsync(List<string> triggerNames)
        {
            List<TriggerMetadata> triggers = [];

            if (triggerNames == null || !triggerNames.Any())
            {
                return triggers;
            }

            using (IDbConnection connection = await _connectionFactory.CreateConnectionAsync())
            {
                List<string> triggerNamesList = triggerNames.ToList();
                string parameters = string.Join(",", triggerNamesList.Select((_, i) => $":p{i}"));
                string query = string.IsNullOrEmpty(_owner)
                    ? $@"SELECT TRIGGER_NAME, TRIGGER_TYPE, TRIGGERING_EVENT, TABLE_NAME, DESCRIPTION 
                         FROM USER_TRIGGERS WHERE UPPER(TRIGGER_NAME) IN ({parameters})"
                    : $@"SELECT TRIGGER_NAME, TRIGGER_TYPE, TRIGGERING_EVENT, TABLE_NAME, DESCRIPTION 
                         FROM ALL_TRIGGERS WHERE UPPER(TRIGGER_NAME) IN ({parameters}) AND OWNER = :Owner";

                using IDbCommand command = connection.CreateCommand();
                command.CommandText = query;

                // Add parameters
                for (int i = 0; i < triggerNamesList.Count; i++)
                {
                    IDbDataParameter parameter = command.CreateParameter();
                    parameter.ParameterName = $"p{i}";
                    parameter.Value = triggerNamesList[i].ToUpper();
                    _ = command.Parameters.Add(parameter);
                }

                // Add owner parameter if owner is specified
                if (!string.IsNullOrEmpty(_owner))
                {
                    IDbDataParameter ownerParam = command.CreateParameter();
                    ownerParam.ParameterName = "Owner";
                    ownerParam.Value = _owner;
                    _ = command.Parameters.Add(ownerParam);
                }

                using IDataReader reader = command.ExecuteReader();
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

            return triggers;
        }

    }
}

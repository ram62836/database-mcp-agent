using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using DatabaseMcp.Core.Interfaces;
using DatabaseMcp.Core.Models;
using Microsoft.Extensions.Logging;

namespace DatabaseMcp.Core.Services
{
    public class SynonymListingService : ISynonymListingService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<SynonymListingService> _logger;
        private readonly string _owner;

        public SynonymListingService(IDbConnectionFactory connectionFactory, ILogger<SynonymListingService> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger;
            _owner = Environment.GetEnvironmentVariable("SchemaOwner");
        }

        public async Task<List<SynonymMetadata>> ListSynonymsAsync()
        {
            _logger.LogInformation("Listing synonyms.");
            List<SynonymMetadata> synonyms = [];
            try
            {
                using (IDbConnection connection = await _connectionFactory.CreateConnectionAsync())
                {
                    string query = string.IsNullOrEmpty(_owner) 
                        ? @"SELECT SYNONYM_NAME, TABLE_OWNER, TABLE_NAME FROM ALL_SYNONYMS"
                        : @"SELECT SYNONYM_NAME, TABLE_OWNER, TABLE_NAME FROM ALL_SYNONYMS WHERE OWNER = :Owner";

                    using IDbCommand command = connection.CreateCommand();
                    command.CommandText = query;
                    
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
                        synonyms.Add(new SynonymMetadata
                        {
                            SynonymName = reader["SYNONYM_NAME"].ToString(),
                            TableOwner = reader["TABLE_OWNER"].ToString(),
                            BaseObjectName = reader["TABLE_NAME"].ToString()
                        });
                    }
                }
                _logger.LogInformation("Retrieved {Count} synonyms.", synonyms.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing synonyms.");
                throw;
            }
            return synonyms;
        }
    }
}
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

        public SynonymListingService(IDbConnectionFactory connectionFactory, ILogger<SynonymListingService> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger;
        }

        public async Task<List<SynonymMetadata>> ListSynonymsAsync()
        {
            _logger.LogInformation("Listing synonyms.");
            List<SynonymMetadata> synonyms = [];
            try
            {
                using (IDbConnection connection = await _connectionFactory.CreateConnectionAsync())
                {
                    string query = @"SELECT SYNONYM_NAME, TABLE_OWNER, TABLE_NAME FROM ALL_SYNONYMS";

                    using IDbCommand command = connection.CreateCommand();
                    command.CommandText = query;
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
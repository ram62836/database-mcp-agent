using System;
using System.Collections.Generic;
using OracleAgent.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using OracleAgent.Core.Models;
using System.Data;

namespace OracleAgent.Core.Services
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
            var synonyms = new List<SynonymMetadata>();
            try
            {
                using (var connection = await _connectionFactory.CreateConnectionAsync())
                {
                    var query = @"SELECT SYNONYM_NAME, TABLE_OWNER, TABLE_NAME FROM ALL_SYNONYMS";

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = query;
                        using (var reader = command.ExecuteReader())
                        {
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
using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using OracleAgent.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using OracleAgent.Core.Models;

namespace OracleAgent.Core.Services
{
    public class SynonymListingService : ISynonymListingService
    {
        private readonly string _connectionString;
        private readonly ILogger<SynonymListingService> _logger;

        public SynonymListingService(IConfiguration config, ILogger<SynonymListingService> logger)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public async Task<List<SynonymMetadata>> ListSynonymsAsync()
        {
            _logger.LogInformation("Listing synonyms.");
            var synonyms = new List<SynonymMetadata>();
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = @"SELECT SYNONYM_NAME, TABLE_OWNER, TABLE_NAME
                                   FROM ALL_SYNONYMS";

                    using (var command = new OracleCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
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
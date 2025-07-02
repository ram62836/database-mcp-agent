using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using OracleAgent.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using OracleAgent.Core.Models;

namespace OracleAgent.Core.Services
{
    public class SynonymListingService : ISynonymListingService
    {
        private readonly string _connectionString;

        public SynonymListingService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<List<SynonymMetadata>> ListSynonymsAsync()
        {
            var synonyms = new List<SynonymMetadata>();

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

            return synonyms;
        }
    }
}
using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using OracleAgent.Core.Models;
using OracleAgent.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace OracleAgent.Core.Services
{
    public class ObjectRelationshipService : IObjectRelationshipService
    {
        private readonly string _connectionString;

        public ObjectRelationshipService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<List<ObjectRelationshipMetadata>> GetReferenceObjects(string objectName, string objectType)
        {
            if (string.IsNullOrWhiteSpace(objectName))
                throw new ArgumentException("object name cannot be null or empty.", nameof(objectName));

            if (string.IsNullOrWhiteSpace(objectType))
                throw new ArgumentException("object type cannot be null or empty.", nameof(objectType));

            var objectRelationShips = new List<ObjectRelationshipMetadata>();
            var query = @"SELECT DISTINCT
                            d.name AS OBJECT_NAME,
                            d.type AS OBJECT_TYPE
                        FROM
                            user_dependencies  d
                        WHERE
                            d.referenced_name = UPPER(:objectName)
                            AND d.referenced_type = UPPER(:objectType)        -- Exclude system-owned objects
                            AND d.name NOT LIKE 'BIN$%'                       -- Exclude dropped (flashback) objects
                            AND d.referenced_owner NOT IN ('SYS', 'SYSTEM')   -- Exclude system-owned referenced objects
                        ORDER BY
                            d.name, d.type";
            using (var connection = new OracleConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new OracleCommand(query, connection))
                {
                    command.Parameters.Add(new OracleParameter("objectName", objectName.ToUpper()));
                    command.Parameters.Add(new OracleParameter("objectType", objectType.ToUpper()));
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            objectRelationShips.Add(new ObjectRelationshipMetadata
                            {
                                ObjectName = reader["OBJECT_NAME"].ToString(),
                                ObjectType = reader["OBJECT_TYPE"].ToString()
                            });
                        }
                    }
                }
            }

            return objectRelationShips;
        }

    }
}
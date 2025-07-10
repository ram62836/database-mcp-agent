using System;
using System.Collections.Generic;
using OracleAgent.Core.Models;
using OracleAgent.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Data;

namespace OracleAgent.Core.Services
{
    public class ObjectRelationshipService : IObjectRelationshipService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<ObjectRelationshipService> _logger;

        public ObjectRelationshipService(IDbConnectionFactory connectionFactory, ILogger<ObjectRelationshipService> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger;
        }

        public async Task<List<ObjectRelationshipMetadata>> GetReferenceObjects(string objectName, string objectType)
        {
            _logger.LogInformation("Getting reference objects for: {ObjectName} of type {ObjectType}", objectName, objectType);
            if (string.IsNullOrWhiteSpace(objectName))
                throw new ArgumentException("object name cannot be null or empty.", nameof(objectName));

            if (string.IsNullOrWhiteSpace(objectType))
                throw new ArgumentException("object type cannot be null or empty.", nameof(objectType));

            var objectRelationShips = new List<ObjectRelationshipMetadata>();
            var query = @"SELECT DISTINCT d.name AS OBJECT_NAME, d.type AS OBJECT_TYPE FROM user_dependencies d WHERE d.referenced_name = UPPER(:objectName) AND d.referenced_type = UPPER(:objectType) AND d.name NOT LIKE 'BIN$%' AND d.referenced_owner NOT IN ('SYS', 'SYSTEM') ORDER BY d.name, d.type";
            try
            {
                using (var connection = await _connectionFactory.CreateConnectionAsync())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = query;
                        var param1 = command.CreateParameter();
                        param1.ParameterName = "objectName";
                        param1.Value = objectName.ToUpper();
                        command.Parameters.Add(param1);
                        var param2 = command.CreateParameter();
                        param2.ParameterName = "objectType";
                        param2.Value = objectType.ToUpper();
                        command.Parameters.Add(param2);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
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
                _logger.LogInformation("Retrieved {Count} reference objects for {ObjectName} of type {ObjectType}", objectRelationShips.Count, objectName, objectType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reference objects for: {ObjectName} of type {ObjectType}", objectName, objectType);
                throw;
            }
            return objectRelationShips;
        }
    }
}
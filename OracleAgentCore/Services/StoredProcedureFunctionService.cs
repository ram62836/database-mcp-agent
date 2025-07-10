using System.Collections.Generic;
using OracleAgent.Core.Interfaces;
using OracleAgent.Core.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.IO;
using System.Linq;
using System.Data;

namespace OracleAgent.Core.Services
{
    public class StoredProcedureFunctionService : IStoredProcedureFunctionService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<StoredProcedureFunctionService> _logger;
        private readonly IMemoryCache _cache;

        public StoredProcedureFunctionService(IDbConnectionFactory connectionFactory, IMemoryCache cache, ILogger<StoredProcedureFunctionService> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<ProcedureFunctionMetadata>> GetAllStoredProceduresAsync()
        {
            _logger.LogInformation("Getting all stored procedures.");
            if (File.Exists(AppConstants.ProceduresMetadatJsonFile)) 
            {
                var fileContent = await File.ReadAllTextAsync(AppConstants.ProceduresMetadatJsonFile);
                List<ProcedureFunctionMetadata> cachedProceduresMetadata = JsonSerializer.Deserialize<List<ProcedureFunctionMetadata>>(fileContent);
                _logger.LogInformation("Loaded {Count} stored procedures from cache.", cachedProceduresMetadata?.Count ?? 0);
                return cachedProceduresMetadata;
            }

            var proceduresMetadata = new List<ProcedureFunctionMetadata>();
            try
            {
                using (var connection = await _connectionFactory.CreateConnectionAsync())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = @"SELECT OBJECT_NAME, OBJECT_TYPE FROM USER_OBJECTS WHERE OBJECT_TYPE = 'PROCEDURE' AND STATUS = 'VALID'";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            proceduresMetadata.Add(new ProcedureFunctionMetadata
                            {
                                Name = reader["OBJECT_NAME"].ToString(),
                                Definition = await GetProcedureDefinitionAsync(reader["OBJECT_NAME"].ToString())
                            });
                        }
                    }
                }
                _logger.LogInformation("Retrieved {Count} stored procedures.", proceduresMetadata.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all stored procedures.");
                throw;
            }
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(proceduresMetadata, options);
            await File.WriteAllTextAsync(AppConstants.ProceduresMetadatJsonFile, json);
            return proceduresMetadata;
        }

        public async Task<List<ProcedureFunctionMetadata>> GetAllFunctionsAsync()
        {
            _logger.LogInformation("Getting all functions.");
            if (File.Exists(AppConstants.FunctionsMetadataJsonFile))
            {
                var fileContent = await File.ReadAllTextAsync(AppConstants.FunctionsMetadataJsonFile);
                List<ProcedureFunctionMetadata> cachedFunctionsMetadata = JsonSerializer.Deserialize<List<ProcedureFunctionMetadata>>(fileContent);
                _logger.LogInformation("Loaded {Count} functions from cache.", cachedFunctionsMetadata?.Count ?? 0);
                return cachedFunctionsMetadata;
            }

            var functionsMetadata = new List<ProcedureFunctionMetadata>();
            try
            {
                using (var connection = await _connectionFactory.CreateConnectionAsync())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = @"SELECT OBJECT_NAME, OBJECT_TYPE FROM USER_OBJECTS WHERE OBJECT_TYPE = 'FUNCTION' AND STATUS = 'VALID'";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            functionsMetadata.Add(new ProcedureFunctionMetadata
                            {
                                Name = reader["OBJECT_NAME"].ToString(),
                                Definition = await GetFunctionDefinitionAsync(reader["OBJECT_NAME"].ToString())
                            });
                        }
                    }
                }
                _logger.LogInformation("Retrieved {Count} functions.", functionsMetadata.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all functions.");
                throw;
            }
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(functionsMetadata, options);
            await File.WriteAllTextAsync(AppConstants.FunctionsMetadataJsonFile, json);
            return functionsMetadata;
        }

        public async Task<List<ProcedureFunctionMetadata>> GetStoredProceduresMetadataByNameAsync(List<string> names)
        {
            _logger.LogInformation("Getting stored procedures by name.");
            var proceduresMetadata = await GetAllStoredProceduresAsync();
            var filtredprocedures = proceduresMetadata
                .Where(procedure => names.Contains(procedure.Name, StringComparer.OrdinalIgnoreCase))
                .ToList();
            _logger.LogInformation("Filtered to {Count} stored procedures by name.", filtredprocedures.Count);
            return filtredprocedures;
        }

        public async Task<List<ProcedureFunctionMetadata>> GetFunctionsMetadataByNameAsync(List<string> names)
        {
            _logger.LogInformation("Getting functions by name.");
            var functionsMetadata = await GetAllFunctionsAsync();
            var filteredfunctions = functionsMetadata
                .Where(function => names.Contains(function.Name, StringComparer.OrdinalIgnoreCase))
                .ToList();
            _logger.LogInformation("Filtered to {Count} functions by name.", filteredfunctions.Count);
            return filteredfunctions;
        }

        public async Task<List<ParameterMetadata>> GetStoredProcedureParametersAsync(string storedProcedureName)
        {
            if (string.IsNullOrEmpty(storedProcedureName))
            {
                throw new ArgumentException("The value cannot be an empty string", nameof(storedProcedureName));
            }
            
            _logger.LogInformation("Getting parameters for stored procedure: {StoredProcedureName}", storedProcedureName);
            return await GetParametersAsync(storedProcedureName);
        }

        public async Task<List<ParameterMetadata>> GetFunctionParametersAsync(string functionName)
        {
            if (string.IsNullOrEmpty(functionName))
            {
                throw new ArgumentException("The value cannot be an empty string", nameof(functionName));
            }
            
            _logger.LogInformation("Getting parameters for function: {FunctionName}", functionName);
            return await GetParametersAsync(functionName);
        }

        private async Task<List<ParameterMetadata>> GetParametersAsync(string objectName)
        {
            if (string.IsNullOrEmpty(objectName))
            {
                throw new ArgumentException("The value cannot be an empty string", nameof(objectName));
            }
            
            _logger.LogInformation("Getting parameters for object: {ObjectName}", objectName);
            var parameters = new List<ParameterMetadata>();
            try
            {
                using (var connection = await _connectionFactory.CreateConnectionAsync())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = @"SELECT ARGUMENT_NAME, DATA_TYPE, IN_OUT FROM ALL_ARGUMENTS WHERE OBJECT_NAME = :objectName AND PACKAGE_NAME IS NULL";
                    var param = command.CreateParameter();
                    param.ParameterName = "objectName";
                    param.Value = objectName;
                    command.Parameters.Add(param);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            parameters.Add(new ParameterMetadata
                            {
                                Name = reader["ARGUMENT_NAME"].ToString(),
                                DataType = reader["DATA_TYPE"].ToString(),
                                Direction = reader["IN_OUT"].ToString()
                            });
                        }
                    }
                }
                _logger.LogInformation("Retrieved {Count} parameters for object {ObjectName}", parameters.Count, objectName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parameters for object: {ObjectName}", objectName);
                throw;
            }
            return parameters;
        }

        private async Task<string> GetProcedureDefinitionAsync(string procedureName)
        {
            if (string.IsNullOrEmpty(procedureName))
            {
                throw new ArgumentException("The value cannot be an empty string", nameof(procedureName));
            }
            
            _logger.LogInformation("Getting procedure definition for: {ProcedureName}", procedureName);
            try
            {
                using (var connection = await _connectionFactory.CreateConnectionAsync())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = @"SELECT DBMS_METADATA.GET_DDL('PROCEDURE', :ProcedureName) AS DDL FROM DUAL";
                    var param = command.CreateParameter();
                    param.ParameterName = "ProcedureName";
                    param.Value = procedureName;
                    command.Parameters.Add(param);

                    return command.ExecuteScalar()?.ToString() ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting procedure definition for: {ProcedureName}", procedureName);
                throw;
            }
        }

        private async Task<string> GetFunctionDefinitionAsync(string functionName)
        {
            if (string.IsNullOrEmpty(functionName))
            {
                throw new ArgumentException("The value cannot be an empty string", nameof(functionName));
            }
            
            _logger.LogInformation("Getting function definition for: {FunctionName}", functionName);
            try
            {
                using (var connection = await _connectionFactory.CreateConnectionAsync())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = @"SELECT DBMS_METADATA.GET_DDL('FUNCTION', :FunctionName) AS DDL FROM DUAL";
                    var param = command.CreateParameter();
                    param.ParameterName = "FunctionName";
                    param.Value = functionName;
                    command.Parameters.Add(param);

                    return command.ExecuteScalar()?.ToString() ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting function definition for: {FunctionName}", functionName);
                throw;
            }
        }
    }
}
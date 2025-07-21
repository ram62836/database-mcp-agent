using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DatabaseMcp.Core.Interfaces;
using DatabaseMcp.Core.Models;
using Microsoft.Extensions.Configuration;

namespace DatabaseMcp.Core.Services
{
    public class StoredProcedureFunctionService : IStoredProcedureFunctionService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<StoredProcedureFunctionService> _logger;        
        private readonly string _metadataJsonDirectory;

        public StoredProcedureFunctionService(IDbConnectionFactory connectionFactory, IConfiguration config, ILogger<StoredProcedureFunctionService> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));        
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _metadataJsonDirectory = config["MetadataJsonPath"] ?? AppConstants.ExecutableDirectory;
        }

        public async Task<List<ProcedureFunctionMetadata>> GetAllStoredProceduresAsync()
        {
            _logger.LogInformation("Getting all stored procedures.");            
            if (File.Exists(AppConstants.ProceduresMetadatJsonFile))
            {
                string fileContent = await File.ReadAllTextAsync(AppConstants.ProceduresMetadatJsonFile);
                List<ProcedureFunctionMetadata> cachedProceduresMetadata = JsonSerializer.Deserialize<List<ProcedureFunctionMetadata>>(fileContent);
                _logger.LogInformation("Loaded {Count} stored procedures from cache.", cachedProceduresMetadata?.Count ?? 0);
                return cachedProceduresMetadata;
            }

            List<ProcedureFunctionMetadata> proceduresMetadata = new();
            try
            {
                using (IDbConnection connection = await _connectionFactory.CreateConnectionAsync())
                {
                    IDbCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT OBJECT_NAME, OBJECT_TYPE FROM USER_OBJECTS WHERE OBJECT_TYPE = 'PROCEDURE' AND STATUS = 'VALID'";
                    using IDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        proceduresMetadata.Add(new ProcedureFunctionMetadata
                        {
                            Name = reader["OBJECT_NAME"].ToString(),
                            Definition = await GetProcedureDefinitionAsync(reader["OBJECT_NAME"].ToString())
                        });
                    }
                }
                _logger.LogInformation("Retrieved {Count} stored procedures.", proceduresMetadata.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all stored procedures.");
                throw;
            }
            JsonSerializerOptions options = new() { WriteIndented = true };
            string json = JsonSerializer.Serialize(proceduresMetadata, options);
            Directory.CreateDirectory(AppConstants.ExecutableDirectory);
            await File.WriteAllTextAsync(AppConstants.ProceduresMetadatJsonFile, json);
            return proceduresMetadata;
        }

        public async Task<List<ProcedureFunctionMetadata>> GetAllFunctionsAsync()
        {
            _logger.LogInformation("Getting all functions.");            
            if (File.Exists(AppConstants.FunctionsMetadataJsonFile))
            {
                string fileContent = await File.ReadAllTextAsync(AppConstants.FunctionsMetadataJsonFile);
                List<ProcedureFunctionMetadata> cachedFunctionsMetadata = JsonSerializer.Deserialize<List<ProcedureFunctionMetadata>>(fileContent);
                _logger.LogInformation("Loaded {Count} functions from cache.", cachedFunctionsMetadata?.Count ?? 0);
                return cachedFunctionsMetadata;
            }

            List<ProcedureFunctionMetadata> functionsMetadata = new();
            try
            {
                using (IDbConnection connection = await _connectionFactory.CreateConnectionAsync())
                {
                    IDbCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT OBJECT_NAME, OBJECT_TYPE FROM USER_OBJECTS WHERE OBJECT_TYPE = 'FUNCTION' AND STATUS = 'VALID'";
                    using IDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        functionsMetadata.Add(new ProcedureFunctionMetadata
                        {
                            Name = reader["OBJECT_NAME"].ToString(),
                            Definition = await GetFunctionDefinitionAsync(reader["OBJECT_NAME"].ToString())
                        });
                    }
                }
                _logger.LogInformation("Retrieved {Count} functions.", functionsMetadata.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all functions.");
                throw;
            }
            JsonSerializerOptions options = new() { WriteIndented = true };
            string json = JsonSerializer.Serialize(functionsMetadata, options);
            Directory.CreateDirectory(AppConstants.ExecutableDirectory);
            await File.WriteAllTextAsync(AppConstants.FunctionsMetadataJsonFile, json);
            return functionsMetadata;
        }

        public async Task<List<ProcedureFunctionMetadata>> GetStoredProceduresMetadataByNameAsync(List<string> names)
        {
            _logger.LogInformation("Getting stored procedures by name.");
            List<ProcedureFunctionMetadata> proceduresMetadata = await GetAllStoredProceduresAsync();
            List<ProcedureFunctionMetadata> filtredprocedures = proceduresMetadata
                .Where(procedure => names.Contains(procedure.Name, StringComparer.OrdinalIgnoreCase))
                .ToList();
            _logger.LogInformation("Filtered to {Count} stored procedures by name.", filtredprocedures.Count);
            return filtredprocedures;
        }

        public async Task<List<ProcedureFunctionMetadata>> GetFunctionsMetadataByNameAsync(List<string> names)
        {
            _logger.LogInformation("Getting functions by name.");
            List<ProcedureFunctionMetadata> functionsMetadata = await GetAllFunctionsAsync();
            List<ProcedureFunctionMetadata> filteredfunctions = functionsMetadata
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
            List<ParameterMetadata> parameters = new();
            try
            {
                using (IDbConnection connection = await _connectionFactory.CreateConnectionAsync())
                {
                    IDbCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT ARGUMENT_NAME, DATA_TYPE, IN_OUT FROM ALL_ARGUMENTS WHERE OBJECT_NAME = :objectName AND PACKAGE_NAME IS NULL";
                    IDbDataParameter param = command.CreateParameter();
                    param.ParameterName = "objectName";
                    param.Value = objectName;
                    _ = command.Parameters.Add(param);

                    using IDataReader reader = command.ExecuteReader();
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
                using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();
                IDbCommand command = connection.CreateCommand();
                command.CommandText = @"SELECT DBMS_METADATA.GET_DDL('PROCEDURE', :ProcedureName) AS DDL FROM DUAL";
                IDbDataParameter param = command.CreateParameter();
                param.ParameterName = "ProcedureName";
                param.Value = procedureName;
                _ = command.Parameters.Add(param);

                return command.ExecuteScalar()?.ToString() ?? string.Empty;
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
                using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();
                IDbCommand command = connection.CreateCommand();
                command.CommandText = @"SELECT DBMS_METADATA.GET_DDL('FUNCTION', :FunctionName) AS DDL FROM DUAL";
                IDbDataParameter param = command.CreateParameter();
                param.ParameterName = "FunctionName";
                param.Value = functionName;
                _ = command.Parameters.Add(param);

                return command.ExecuteScalar()?.ToString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting function definition for: {FunctionName}", functionName);
                throw;
            }
        }
    }
}
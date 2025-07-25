using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using DatabaseMcp.Core.Interfaces;
using DatabaseMcp.Core.Models;
using Microsoft.Extensions.Logging;

namespace DatabaseMcp.Core.Services
{
    public class StoredProcedureFunctionService : IStoredProcedureFunctionService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<StoredProcedureFunctionService> _logger;

        public StoredProcedureFunctionService(IDbConnectionFactory connectionFactory, ILogger<StoredProcedureFunctionService> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<ProcedureFunctionMetadata>> GetStoredProceduresMetadataByNamesAsync(List<string> names)
        {
            _logger.LogInformation("Getting stored procedures by name.");
            List<ProcedureFunctionMetadata> proceduresMetadata = [];
            foreach (string procedureName in names)
            {
                ProcedureFunctionMetadata procedure = new()
                {
                    Definition = await GetProcedureDefinitionAsync(procedureName)
                };
                proceduresMetadata.Add(procedure);
            }
            _logger.LogInformation("Found {Count} stored procedures by name.", proceduresMetadata.Count);
            return proceduresMetadata;
        }

        public async Task<List<ProcedureFunctionMetadata>> GetFunctionsMetadataByNamesAsync(List<string> names)
        {
            _logger.LogInformation("Getting functions by name.");
            List<ProcedureFunctionMetadata> functionsMetadata = [];
            foreach (string functionName in names)
            {
                ProcedureFunctionMetadata function = new()
                {
                    Definition = await GetFunctionDefinitionAsync(functionName)
                };
                functionsMetadata.Add(function);
            }

            _logger.LogInformation("Found {Count} functions by name.", functionsMetadata.Count);
            return functionsMetadata;
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
            List<ParameterMetadata> parameters = [];
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
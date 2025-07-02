using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using OracleAgent.Core.Interfaces;
using OracleAgent.Core.Models;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Text.Json;
using System.IO;
using System.Linq;

namespace OracleAgent.Core.Services
{
    public class StoredProcedureFunctionService : IStoredProcedureFunctionService
    {
        private readonly string _connectionString;        

        public StoredProcedureFunctionService(IConfiguration config, IMemoryCache cache)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<List<ProcedureFunctionMetadata>> GetAllStoredProceduresAsync()
        {
            if (File.Exists(AppConstants.ProceduresMetadatJsonFile)) 
            {
                var fileContent = await File.ReadAllTextAsync(AppConstants.ProceduresMetadatJsonFile);
                List<ProcedureFunctionMetadata> cachedProceduresMetadata = JsonSerializer.Deserialize<List<ProcedureFunctionMetadata>>(fileContent);
                return cachedProceduresMetadata;
            }

            var proceduresMetadata = new List<ProcedureFunctionMetadata>();
            using (var connection = new OracleConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new OracleCommand(@"SELECT OBJECT_NAME, OBJECT_TYPE  
                                                   FROM USER_OBJECTS  
                                                   WHERE OBJECT_TYPE = 'PROCEDURE'  
                                                     AND STATUS = 'VALID'", connection);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        proceduresMetadata.Add(new ProcedureFunctionMetadata
                        {
                            Name = reader["OBJECT_NAME"].ToString(),
                            Definition = await GetProcedureDefinitionAsync(reader["OBJECT_NAME"].ToString())
                        });
                    }
                }
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(proceduresMetadata, options);
            await File.WriteAllTextAsync(AppConstants.ProceduresMetadatJsonFile, json);
            return proceduresMetadata;
        }

        public async Task<List<ProcedureFunctionMetadata>> GetAllFunctionsAsync()
        {
            if (File.Exists(AppConstants.FunctionsMetadataJsonFile))
            {
                var fileContent = await File.ReadAllTextAsync(AppConstants.FunctionsMetadataJsonFile);
                List<ProcedureFunctionMetadata> cachedFunctionsMetadata = JsonSerializer.Deserialize<List<ProcedureFunctionMetadata>>(fileContent);
                return cachedFunctionsMetadata;
            }

            var functionsMetadata = new List<ProcedureFunctionMetadata>();
            using (var connection = new OracleConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new OracleCommand(@"SELECT OBJECT_NAME, OBJECT_TYPE
                                                    FROM USER_OBJECTS
                                                    WHERE OBJECT_TYPE = 'FUNCTION'
                                                      AND STATUS = 'VALID'", connection);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        functionsMetadata.Add(new ProcedureFunctionMetadata
                        {
                            Name = reader["OBJECT_NAME"].ToString(),
                            Definition = await GetFunctionDefinitionAsync(reader["OBJECT_NAME"].ToString())
                        });
                    }
                }
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(functionsMetadata, options);
            await File.WriteAllTextAsync(AppConstants.FunctionsMetadataJsonFile, json);
            return functionsMetadata;
        }

        public async Task<List<ProcedureFunctionMetadata>> GetStoredProceduresMetadataByNameAsync(List<string> names)
        {
            var proceduresMetadata = await GetAllStoredProceduresAsync();
            var filtredprocedures = proceduresMetadata
                .Where(procedure => names.Contains(procedure.Name, StringComparer.OrdinalIgnoreCase))
                .ToList();
            return filtredprocedures;
        }

        public async Task<List<ProcedureFunctionMetadata>> GetFunctionsMetadataByNameAsync(List<string> names)
        {
            var functionsMetadata = await GetAllFunctionsAsync();
            var filteredfunctions = functionsMetadata
                .Where(function => names.Contains(function.Name, StringComparer.OrdinalIgnoreCase))
                .ToList();
            return filteredfunctions;
        }

        public async Task<List<ParameterMetadata>> GetStoredProcedureParametersAsync(string storedProcedureName)
        {
            return await GetParametersAsync(storedProcedureName);
        }

        public async Task<List<ParameterMetadata>> GetFunctionParametersAsync(string functionName)
        {
            return await GetParametersAsync(functionName);
        }

        private async Task<List<ParameterMetadata>> GetParametersAsync(string objectName)
        {
            var parameters = new List<ParameterMetadata>();

            using (var connection = new OracleConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new OracleCommand(@"SELECT ARGUMENT_NAME, DATA_TYPE, IN_OUT
                                                   FROM ALL_ARGUMENTS
                                                   WHERE OBJECT_NAME = :objectName AND PACKAGE_NAME IS NULL", connection);
                command.Parameters.Add(new OracleParameter("objectName", objectName));

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
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

            return parameters;
        }

        private async Task<string> GetProcedureDefinitionAsync(string procedureName)
        {
            using (var connection = new OracleConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new OracleCommand(@"SELECT DBMS_METADATA.GET_DDL('PROCEDURE', :ProcedureName) AS DDL
                                                   FROM DUAL", connection);
                command.Parameters.Add(new OracleParameter("ProcedureName", procedureName));

                return (await command.ExecuteScalarAsync())?.ToString() ?? string.Empty;
            }
        }

        private async Task<string> GetFunctionDefinitionAsync(string functionName)
        {
            using (var connection = new OracleConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new OracleCommand(@"SELECT DBMS_METADATA.GET_DDL('FUNCTION', :FunctionName) AS DDL
                                                   FROM DUAL", connection);
                command.Parameters.Add(new OracleParameter("FunctionName", functionName));

                return (await command.ExecuteScalarAsync())?.ToString() ?? string.Empty;
            }
        }
    }
}
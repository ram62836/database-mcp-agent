using Hala.DatabaseAgent.Core.Interfaces;
using Hala.DatabaseAgent.Core.Models;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace Hala.DatabaseAgent.Oracle
{
    /// <summary>
    /// Oracle implementation of stored procedure and function service
    /// </summary>
    public class OracleStoredProcedureFunctionService : IStoredProcedureFunctionService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<OracleStoredProcedureFunctionService> _logger;

        // Cache for metadata
        private Dictionary<string, object> _metadataCache = new Dictionary<string, object>();
        private Dictionary<string, DateTime> _cacheExpiration = new Dictionary<string, DateTime>();
        private readonly TimeSpan _defaultCacheDuration = TimeSpan.FromMinutes(60);

        /// <summary>
        /// Initializes a new instance of the <see cref="OracleStoredProcedureFunctionService"/> class.
        /// </summary>
        /// <param name="connectionFactory">The connection factory.</param>
        /// <param name="logger">The logger.</param>
        public OracleStoredProcedureFunctionService(
            IDbConnectionFactory connectionFactory,
            ILogger<OracleStoredProcedureFunctionService> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<List<StoredProcedureMetadata>> GetAllStoredProceduresAsync()
        {
            const string cacheKey = "all_procedures";
            
            if (TryGetFromCache<List<StoredProcedureMetadata>>(cacheKey, out var cachedProcedures))
            {
                return cachedProcedures;
            }
            
            var procedures = new List<StoredProcedureMetadata>();
            
            string schema = _connectionFactory.GetDefaultSchema().ToUpper();
            string sql = @"
                SELECT 
                    p.OBJECT_NAME AS PROCEDURE_NAME,
                    p.OWNER,
                    TO_CHAR(p.CREATED, 'YYYY-MM-DD HH24:MI:SS') AS CREATED_DATE,
                    TO_CHAR(p.LAST_DDL_TIME, 'YYYY-MM-DD HH24:MI:SS') AS LAST_DDL_TIME
                FROM 
                    ALL_OBJECTS p
                WHERE 
                    p.OWNER = :schema 
                    AND p.OBJECT_TYPE = 'PROCEDURE'
                ORDER BY 
                    p.OBJECT_NAME";

            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync() as OracleConnection;
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("schema", OracleDbType.Varchar2) { Value = schema });
                
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var procedureName = reader["PROCEDURE_NAME"].ToString();
                    var owner = reader["OWNER"].ToString();
                    
                    var procedure = new StoredProcedureMetadata
                    {
                        ProcedureName = procedureName,
                        Owner = owner,
                        CreatedDate = reader["CREATED_DATE"] != DBNull.Value ? DateTime.Parse(reader["CREATED_DATE"].ToString()) : null,
                        LastModifiedDate = reader["LAST_DDL_TIME"] != DBNull.Value ? DateTime.Parse(reader["LAST_DDL_TIME"].ToString()) : null,
                        Definition = await GetProcedureDefinitionAsync(owner, procedureName),
                        Parameters = await GetProcedureParametersAsync(owner, procedureName)
                    };
                    
                    procedures.Add(procedure);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Oracle stored procedures");
                throw;
            }
            
            AddToCache(cacheKey, procedures, _defaultCacheDuration);
            return procedures;
        }

        /// <inheritdoc/>
        public async Task<StoredProcedureMetadata> GetStoredProcedureMetadataAsync(string schema, string procedureName)
        {
            string cacheKey = $"procedure_{schema}_{procedureName}";
            
            if (TryGetFromCache<StoredProcedureMetadata>(cacheKey, out var cachedProcedure))
            {
                return cachedProcedure;
            }
            
            string sql = @"
                SELECT 
                    p.OBJECT_NAME AS PROCEDURE_NAME,
                    p.OWNER,
                    TO_CHAR(p.CREATED, 'YYYY-MM-DD HH24:MI:SS') AS CREATED_DATE,
                    TO_CHAR(p.LAST_DDL_TIME, 'YYYY-MM-DD HH24:MI:SS') AS LAST_DDL_TIME
                FROM 
                    ALL_OBJECTS p
                WHERE 
                    p.OWNER = :schema 
                    AND p.OBJECT_NAME = :procedureName
                    AND p.OBJECT_TYPE = 'PROCEDURE'";

            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync() as OracleConnection;
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("schema", OracleDbType.Varchar2) { Value = schema.ToUpper() });
                command.Parameters.Add(new OracleParameter("procedureName", OracleDbType.Varchar2) { Value = procedureName.ToUpper() });
                
                using var reader = await command.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    return null;
                }
                
                var procedure = new StoredProcedureMetadata
                {
                    ProcedureName = reader["PROCEDURE_NAME"].ToString(),
                    Owner = reader["OWNER"].ToString(),
                    CreatedDate = reader["CREATED_DATE"] != DBNull.Value ? DateTime.Parse(reader["CREATED_DATE"].ToString()) : null,
                    LastModifiedDate = reader["LAST_DDL_TIME"] != DBNull.Value ? DateTime.Parse(reader["LAST_DDL_TIME"].ToString()) : null,
                    Definition = await GetProcedureDefinitionAsync(schema, procedureName),
                    Parameters = await GetProcedureParametersAsync(schema, procedureName)
                };
                
                AddToCache(cacheKey, procedure, _defaultCacheDuration);
                return procedure;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Oracle stored procedure metadata for {Schema}.{ProcedureName}", schema, procedureName);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<FunctionMetadata>> GetAllFunctionsAsync()
        {
            const string cacheKey = "all_functions";
            
            if (TryGetFromCache<List<FunctionMetadata>>(cacheKey, out var cachedFunctions))
            {
                return cachedFunctions;
            }
            
            var functions = new List<FunctionMetadata>();
            
            string schema = _connectionFactory.GetDefaultSchema().ToUpper();
            string sql = @"
                SELECT 
                    f.OBJECT_NAME AS FUNCTION_NAME,
                    f.OWNER,
                    TO_CHAR(f.CREATED, 'YYYY-MM-DD HH24:MI:SS') AS CREATED_DATE,
                    TO_CHAR(f.LAST_DDL_TIME, 'YYYY-MM-DD HH24:MI:SS') AS LAST_DDL_TIME
                FROM 
                    ALL_OBJECTS f
                WHERE 
                    f.OWNER = :schema 
                    AND f.OBJECT_TYPE = 'FUNCTION'
                ORDER BY 
                    f.OBJECT_NAME";

            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync() as OracleConnection;
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("schema", OracleDbType.Varchar2) { Value = schema });
                
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var functionName = reader["FUNCTION_NAME"].ToString();
                    var owner = reader["OWNER"].ToString();
                    
                    var functionInfo = new FunctionMetadata
                    {
                        FunctionName = functionName,
                        Owner = owner,
                        CreatedDate = reader["CREATED_DATE"] != DBNull.Value ? DateTime.Parse(reader["CREATED_DATE"].ToString()) : null,
                        LastModifiedDate = reader["LAST_DDL_TIME"] != DBNull.Value ? DateTime.Parse(reader["LAST_DDL_TIME"].ToString()) : null,
                        Definition = await GetFunctionDefinitionAsync(owner, functionName),
                        ReturnType = await GetFunctionReturnTypeAsync(owner, functionName),
                        Parameters = await GetFunctionParametersAsync(owner, functionName)
                    };
                    
                    functions.Add(functionInfo);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Oracle functions");
                throw;
            }
            
            AddToCache(cacheKey, functions, _defaultCacheDuration);
            return functions;
        }

        /// <inheritdoc/>
        public async Task<FunctionMetadata> GetFunctionMetadataAsync(string schema, string functionName)
        {
            string cacheKey = $"function_{schema}_{functionName}";
            
            if (TryGetFromCache<FunctionMetadata>(cacheKey, out var cachedFunction))
            {
                return cachedFunction;
            }
            
            string sql = @"
                SELECT 
                    f.OBJECT_NAME AS FUNCTION_NAME,
                    f.OWNER,
                    TO_CHAR(f.CREATED, 'YYYY-MM-DD HH24:MI:SS') AS CREATED_DATE,
                    TO_CHAR(f.LAST_DDL_TIME, 'YYYY-MM-DD HH24:MI:SS') AS LAST_DDL_TIME
                FROM 
                    ALL_OBJECTS f
                WHERE 
                    f.OWNER = :schema 
                    AND f.OBJECT_NAME = :functionName
                    AND f.OBJECT_TYPE = 'FUNCTION'";

            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync() as OracleConnection;
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("schema", OracleDbType.Varchar2) { Value = schema.ToUpper() });
                command.Parameters.Add(new OracleParameter("functionName", OracleDbType.Varchar2) { Value = functionName.ToUpper() });
                
                using var reader = await command.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    return null;
                }
                
                var function = new FunctionMetadata
                {
                    FunctionName = reader["FUNCTION_NAME"].ToString(),
                    Owner = reader["OWNER"].ToString(),
                    CreatedDate = reader["CREATED_DATE"] != DBNull.Value ? DateTime.Parse(reader["CREATED_DATE"].ToString()) : null,
                    LastModifiedDate = reader["LAST_DDL_TIME"] != DBNull.Value ? DateTime.Parse(reader["LAST_DDL_TIME"].ToString()) : null,
                    Definition = await GetFunctionDefinitionAsync(schema, functionName),
                    ReturnType = await GetFunctionReturnTypeAsync(schema, functionName),
                    Parameters = await GetFunctionParametersAsync(schema, functionName)
                };
                
                AddToCache(cacheKey, function, _defaultCacheDuration);
                return function;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Oracle function metadata for {Schema}.{FunctionName}", schema, functionName);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<SqlQueryResult> ExecuteStoredProcedureAsync(string schema, string procedureName, Dictionary<string, object> parameters)
        {
            var result = new SqlQueryResult();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync() as OracleConnection;
                using var command = connection.CreateCommand();
                command.CommandText = $"{schema}.{procedureName}";
                command.CommandType = CommandType.StoredProcedure;
                
                // Create parameters
                var procedureParams = await GetProcedureParametersAsync(schema, procedureName);
                var outputParams = new Dictionary<string, OracleParameter>();
                
                foreach (var param in procedureParams)
                {
                    var oracleParam = new OracleParameter(param.ParameterName, MapToOracleType(param.DataType));
                    
                    if (param.ParameterMode == "IN" || param.ParameterMode == "INOUT")
                    {
                        if (parameters.ContainsKey(param.ParameterName))
                        {
                            oracleParam.Value = parameters[param.ParameterName] ?? DBNull.Value;
                        }
                        else
                        {
                            oracleParam.Value = DBNull.Value;
                        }
                    }
                    
                    if (param.ParameterMode == "OUT" || param.ParameterMode == "INOUT")
                    {
                        oracleParam.Direction = param.ParameterMode == "INOUT" ? ParameterDirection.InputOutput : ParameterDirection.Output;
                        outputParams[param.ParameterName] = oracleParam;
                    }
                    
                    command.Parameters.Add(oracleParam);
                }
                
                // Execute
                await command.ExecuteNonQueryAsync();
                
                // Process output parameters
                if (outputParams.Count > 0)
                {
                    result.ColumnNames = outputParams.Keys.ToList();
                    result.ColumnTypes = outputParams.Values.Select(p => p.OracleDbType.ToString()).ToList();
                    
                    var row = new List<object>();
                    foreach (var param in outputParams.Values)
                    {
                        row.Add(param.Value == DBNull.Value ? null : param.Value);
                    }
                    
                    result.Rows.Add(row);
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Error executing Oracle stored procedure {Schema}.{ProcedureName}", schema, procedureName);
            }
            finally
            {
                stopwatch.Stop();
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
            }
            
            return result;
        }

        /// <inheritdoc/>
        public async Task<SqlQueryResult> ExecuteFunctionAsync(string schema, string functionName, Dictionary<string, object> parameters)
        {
            var result = new SqlQueryResult();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync() as OracleConnection;
                
                // Build the function call SQL
                var functionParams = await GetFunctionParametersAsync(schema, functionName);
                var paramPlaceholders = string.Join(", ", functionParams.Select((p, i) => $":p{i}"));
                
                var sql = $"SELECT {schema}.{functionName}({paramPlaceholders}) AS RESULT FROM DUAL";
                
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                
                // Add parameters
                for (int i = 0; i < functionParams.Count; i++)
                {
                    var param = functionParams[i];
                    var oracleParam = new OracleParameter($"p{i}", MapToOracleType(param.DataType));
                    
                    if (parameters.ContainsKey(param.ParameterName))
                    {
                        oracleParam.Value = parameters[param.ParameterName] ?? DBNull.Value;
                    }
                    else
                    {
                        oracleParam.Value = DBNull.Value;
                    }
                    
                    command.Parameters.Add(oracleParam);
                }
                
                // Execute and get result
                using var reader = await command.ExecuteReaderAsync();
                result.ColumnNames.Add("RESULT");
                
                var returnType = await GetFunctionReturnTypeAsync(schema, functionName);
                result.ColumnTypes.Add(returnType);
                
                if (await reader.ReadAsync())
                {
                    var value = reader.IsDBNull(0) ? null : reader.GetValue(0);
                    result.Rows.Add(new List<object> { value });
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Error executing Oracle function {Schema}.{FunctionName}", schema, functionName);
            }
            finally
            {
                stopwatch.Stop();
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
            }
            
            return result;
        }

        #region Private Helper Methods

        private async Task<string> GetProcedureDefinitionAsync(string schema, string procedureName)
        {
            string sql = @"
                SELECT DBMS_METADATA.GET_DDL('PROCEDURE', :objectName, :owner) AS DEFINITION
                FROM DUAL";

            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync() as OracleConnection;
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("objectName", OracleDbType.Varchar2) { Value = procedureName.ToUpper() });
                command.Parameters.Add(new OracleParameter("owner", OracleDbType.Varchar2) { Value = schema.ToUpper() });
                
                var definition = await command.ExecuteScalarAsync() as string;
                return definition;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving definition for procedure {Schema}.{ProcedureName}", schema, procedureName);
                return null;
            }
        }

        private async Task<string> GetFunctionDefinitionAsync(string schema, string functionName)
        {
            string sql = @"
                SELECT DBMS_METADATA.GET_DDL('FUNCTION', :objectName, :owner) AS DEFINITION
                FROM DUAL";

            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync() as OracleConnection;
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("objectName", OracleDbType.Varchar2) { Value = functionName.ToUpper() });
                command.Parameters.Add(new OracleParameter("owner", OracleDbType.Varchar2) { Value = schema.ToUpper() });
                
                var definition = await command.ExecuteScalarAsync() as string;
                return definition;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving definition for function {Schema}.{FunctionName}", schema, functionName);
                return null;
            }
        }

        private async Task<List<ParameterMetadata>> GetProcedureParametersAsync(string schema, string procedureName)
        {
            var parameters = new List<ParameterMetadata>();
            
            string sql = @"
                SELECT 
                    a.ARGUMENT_NAME AS PARAMETER_NAME,
                    a.DATA_TYPE,
                    a.POSITION,
                    a.IN_OUT AS PARAMETER_MODE,
                    a.DATA_LENGTH AS MAX_LENGTH,
                    a.DATA_PRECISION AS PRECISION,
                    a.DATA_SCALE AS SCALE,
                    a.DEFAULT_VALUE
                FROM 
                    ALL_ARGUMENTS a
                WHERE 
                    a.OWNER = :schema 
                    AND a.OBJECT_NAME = :procedureName
                    AND a.ARGUMENT_NAME IS NOT NULL
                ORDER BY 
                    a.POSITION";

            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync() as OracleConnection;
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("schema", OracleDbType.Varchar2) { Value = schema.ToUpper() });
                command.Parameters.Add(new OracleParameter("procedureName", OracleDbType.Varchar2) { Value = procedureName.ToUpper() });
                
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    parameters.Add(new ParameterMetadata
                    {
                        ParameterName = reader["PARAMETER_NAME"].ToString(),
                        DataType = reader["DATA_TYPE"].ToString(),
                        Position = Convert.ToInt32(reader["POSITION"]),
                        ParameterMode = reader["PARAMETER_MODE"].ToString(),
                        MaxLength = reader["MAX_LENGTH"] != DBNull.Value ? Convert.ToInt32(reader["MAX_LENGTH"]) : null,
                        Precision = reader["PRECISION"] != DBNull.Value ? Convert.ToInt32(reader["PRECISION"]) : null,
                        Scale = reader["SCALE"] != DBNull.Value ? Convert.ToInt32(reader["SCALE"]) : null,
                        DefaultValue = reader["DEFAULT_VALUE"] != DBNull.Value ? reader["DEFAULT_VALUE"].ToString() : null
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving parameters for procedure {Schema}.{ProcedureName}", schema, procedureName);
            }
            
            return parameters;
        }

        private async Task<List<ParameterMetadata>> GetFunctionParametersAsync(string schema, string functionName)
        {
            var parameters = new List<ParameterMetadata>();
            
            string sql = @"
                SELECT 
                    a.ARGUMENT_NAME AS PARAMETER_NAME,
                    a.DATA_TYPE,
                    a.POSITION,
                    a.IN_OUT AS PARAMETER_MODE,
                    a.DATA_LENGTH AS MAX_LENGTH,
                    a.DATA_PRECISION AS PRECISION,
                    a.DATA_SCALE AS SCALE,
                    a.DEFAULT_VALUE
                FROM 
                    ALL_ARGUMENTS a
                WHERE 
                    a.OWNER = :schema 
                    AND a.OBJECT_NAME = :functionName
                    AND a.ARGUMENT_NAME IS NOT NULL
                    AND a.POSITION > 0  -- Position 0 is the return value
                ORDER BY 
                    a.POSITION";

            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync() as OracleConnection;
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("schema", OracleDbType.Varchar2) { Value = schema.ToUpper() });
                command.Parameters.Add(new OracleParameter("functionName", OracleDbType.Varchar2) { Value = functionName.ToUpper() });
                
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    parameters.Add(new ParameterMetadata
                    {
                        ParameterName = reader["PARAMETER_NAME"].ToString(),
                        DataType = reader["DATA_TYPE"].ToString(),
                        Position = Convert.ToInt32(reader["POSITION"]),
                        ParameterMode = reader["PARAMETER_MODE"].ToString(),
                        MaxLength = reader["MAX_LENGTH"] != DBNull.Value ? Convert.ToInt32(reader["MAX_LENGTH"]) : null,
                        Precision = reader["PRECISION"] != DBNull.Value ? Convert.ToInt32(reader["PRECISION"]) : null,
                        Scale = reader["SCALE"] != DBNull.Value ? Convert.ToInt32(reader["SCALE"]) : null,
                        DefaultValue = reader["DEFAULT_VALUE"] != DBNull.Value ? reader["DEFAULT_VALUE"].ToString() : null
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving parameters for function {Schema}.{FunctionName}", schema, functionName);
            }
            
            return parameters;
        }

        private async Task<string> GetFunctionReturnTypeAsync(string schema, string functionName)
        {
            string sql = @"
                SELECT 
                    a.DATA_TYPE
                FROM 
                    ALL_ARGUMENTS a
                WHERE 
                    a.OWNER = :schema 
                    AND a.OBJECT_NAME = :functionName
                    AND a.POSITION = 0  -- Position 0 is the return value
                    AND a.IN_OUT = 'OUT'";

            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync() as OracleConnection;
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.Add(new OracleParameter("schema", OracleDbType.Varchar2) { Value = schema.ToUpper() });
                command.Parameters.Add(new OracleParameter("functionName", OracleDbType.Varchar2) { Value = functionName.ToUpper() });
                
                var returnType = await command.ExecuteScalarAsync() as string;
                return returnType ?? "UNKNOWN";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving return type for function {Schema}.{FunctionName}", schema, functionName);
                return "UNKNOWN";
            }
        }

        private OracleDbType MapToOracleType(string dataType)
        {
            switch (dataType.ToUpper())
            {
                case "VARCHAR2":
                case "VARCHAR":
                case "CHAR":
                case "NCHAR":
                case "NVARCHAR2":
                    return OracleDbType.Varchar2;
                case "NUMBER":
                    return OracleDbType.Decimal;
                case "INTEGER":
                case "INT":
                    return OracleDbType.Int32;
                case "FLOAT":
                    return OracleDbType.Single;
                case "BINARY_FLOAT":
                    return OracleDbType.BinaryFloat;
                case "BINARY_DOUBLE":
                    return OracleDbType.BinaryDouble;
                case "DATE":
                    return OracleDbType.Date;
                case "TIMESTAMP":
                    return OracleDbType.TimeStamp;
                case "BLOB":
                    return OracleDbType.Blob;
                case "CLOB":
                case "NCLOB":
                    return OracleDbType.Clob;
                case "BOOLEAN":
                    return OracleDbType.Boolean;
                case "RAW":
                    return OracleDbType.Raw;
                default:
                    return OracleDbType.Varchar2;
            }
        }

        #endregion

        #region Cache Methods

        private void AddToCache<T>(string key, T value, TimeSpan duration)
        {
            _metadataCache[key] = value;
            _cacheExpiration[key] = DateTime.UtcNow.Add(duration);
        }

        private bool TryGetFromCache<T>(string key, out T value)
        {
            value = default;
            
            if (!_metadataCache.ContainsKey(key) || !_cacheExpiration.ContainsKey(key))
            {
                return false;
            }
            
            if (DateTime.UtcNow > _cacheExpiration[key])
            {
                _metadataCache.Remove(key);
                _cacheExpiration.Remove(key);
                return false;
            }
            
            if (_metadataCache[key] is T cachedValue)
            {
                value = cachedValue;
                return true;
            }
            
            return false;
        }

        #endregion
    }
}

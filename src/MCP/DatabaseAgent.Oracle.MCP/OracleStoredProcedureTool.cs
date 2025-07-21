using Hala.DatabaseAgent.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.ModelContextProtocol;
using Microsoft.ModelContextProtocol.Converters;
using Microsoft.ModelContextProtocol.Server;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Hala.DatabaseAgent.Oracle.MCP
{
    [McpTool("stored-procedures", "Oracle Stored Procedures and Functions")]
    [Description("Work with stored procedures and functions in an Oracle database")]
    public class OracleStoredProcedureTool : McpTool
    {
        private readonly IStoredProcedureFunctionService _sprocService;
        private readonly ILogger<OracleStoredProcedureTool> _logger;

        public OracleStoredProcedureTool(IStoredProcedureFunctionService sprocService, ILogger<OracleStoredProcedureTool> logger)
        {
            _sprocService = sprocService ?? throw new ArgumentNullException(nameof(sprocService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [McpToolFunction("list-procedures", "List all stored procedures")]
        [Description("Returns a list of all stored procedures in the database")]
        [McpFunctionOutput("A list of stored procedures with their metadata")]
        public async Task<McpToolResponse> ListProceduresAsync()
        {
            try
            {
                var procedures = await _sprocService.GetAllStoredProceduresAsync();
                return McpToolResponse.FromObject(procedures);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing stored procedures");
                return McpToolResponse.Error($"Error listing stored procedures: {ex.Message}");
            }
        }

        [McpToolFunction("get-procedure", "Get detailed metadata for a specific stored procedure")]
        [Description("Returns detailed metadata for a specific stored procedure including parameters and definition")]
        [McpFunctionOutput("Detailed stored procedure metadata")]
        public async Task<McpToolResponse> GetProcedureAsync(
            [Description("The schema/owner of the stored procedure")] string schema,
            [Description("The name of the stored procedure")] string procedureName)
        {
            try
            {
                var procedure = await _sprocService.GetStoredProcedureMetadataAsync(schema, procedureName);
                if (procedure == null)
                {
                    return McpToolResponse.Error($"Stored procedure {schema}.{procedureName} not found");
                }
                return McpToolResponse.FromObject(procedure);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stored procedure metadata for {Schema}.{ProcedureName}", schema, procedureName);
                return McpToolResponse.Error($"Error getting stored procedure metadata: {ex.Message}");
            }
        }

        [McpToolFunction("execute-procedure", "Execute a stored procedure")]
        [Description("Executes a stored procedure with the provided parameters")]
        [McpFunctionOutput("Results of the stored procedure execution")]
        public async Task<McpToolResponse> ExecuteProcedureAsync(
            [Description("The schema/owner of the stored procedure")] string schema,
            [Description("The name of the stored procedure")] string procedureName,
            [Description("JSON object containing parameter names and values")] string parameters = "{}")
        {
            try
            {
                // Parse parameters
                Dictionary<string, object> paramDict;
                try
                {
                    paramDict = JsonSerializer.Deserialize<Dictionary<string, object>>(parameters) ?? new Dictionary<string, object>();
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Error parsing parameters JSON: {Parameters}", parameters);
                    return McpToolResponse.Error($"Error parsing parameters JSON: {ex.Message}");
                }

                var result = await _sprocService.ExecuteStoredProcedureAsync(schema, procedureName, paramDict);
                return McpToolResponse.FromObject(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing stored procedure {Schema}.{ProcedureName}", schema, procedureName);
                return McpToolResponse.Error($"Error executing stored procedure: {ex.Message}");
            }
        }

        [McpToolFunction("list-functions", "List all functions")]
        [Description("Returns a list of all functions in the database")]
        [McpFunctionOutput("A list of functions with their metadata")]
        public async Task<McpToolResponse> ListFunctionsAsync()
        {
            try
            {
                var functions = await _sprocService.GetAllFunctionsAsync();
                return McpToolResponse.FromObject(functions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing functions");
                return McpToolResponse.Error($"Error listing functions: {ex.Message}");
            }
        }

        [McpToolFunction("get-function", "Get detailed metadata for a specific function")]
        [Description("Returns detailed metadata for a specific function including parameters, return type, and definition")]
        [McpFunctionOutput("Detailed function metadata")]
        public async Task<McpToolResponse> GetFunctionAsync(
            [Description("The schema/owner of the function")] string schema,
            [Description("The name of the function")] string functionName)
        {
            try
            {
                var function = await _sprocService.GetFunctionMetadataAsync(schema, functionName);
                if (function == null)
                {
                    return McpToolResponse.Error($"Function {schema}.{functionName} not found");
                }
                return McpToolResponse.FromObject(function);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting function metadata for {Schema}.{FunctionName}", schema, functionName);
                return McpToolResponse.Error($"Error getting function metadata: {ex.Message}");
            }
        }

        [McpToolFunction("execute-function", "Execute a function")]
        [Description("Executes a function with the provided parameters and returns the result")]
        [McpFunctionOutput("Result of the function execution")]
        public async Task<McpToolResponse> ExecuteFunctionAsync(
            [Description("The schema/owner of the function")] string schema,
            [Description("The name of the function")] string functionName,
            [Description("JSON object containing parameter names and values")] string parameters = "{}")
        {
            try
            {
                // Parse parameters
                Dictionary<string, object> paramDict;
                try
                {
                    paramDict = JsonSerializer.Deserialize<Dictionary<string, object>>(parameters) ?? new Dictionary<string, object>();
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Error parsing parameters JSON: {Parameters}", parameters);
                    return McpToolResponse.Error($"Error parsing parameters JSON: {ex.Message}");
                }

                var result = await _sprocService.ExecuteFunctionAsync(schema, functionName, paramDict);
                return McpToolResponse.FromObject(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing function {Schema}.{FunctionName}", schema, functionName);
                return McpToolResponse.Error($"Error executing function: {ex.Message}");
            }
        }
    }
}

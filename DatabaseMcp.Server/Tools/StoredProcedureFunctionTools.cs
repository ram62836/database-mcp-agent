using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using DatabaseMcp.Core.Interfaces;
using DatabaseMcp.Core.Models;
using ModelContextProtocol.Server;

namespace DatabaseMcp.Server.Tools
{

    [McpServerToolType()]
    public static class StoredProcedureFunctionTools
    {
        [McpServerTool, Description("Retrieves detailed metadata for the given stored procedure by their names.")]
        public static async Task<List<ProcedureFunctionMetadata>> GetStoredProceduresMetadataByNameAsync(
            IStoredProcedureFunctionService service,
            [Description("List of stored procedure names to retrieve metadata for.")] List<string> storedProcedureNames)
        {
            return await service.GetStoredProceduresMetadataByNamesAsync(storedProcedureNames);
        }

        [McpServerTool, Description("Retrieves detailed metadata for the given functions by their names.")]
        public static async Task<List<ProcedureFunctionMetadata>> GetFunctionsMetadataByNameAsync(
            IStoredProcedureFunctionService service,
            [Description("List of functions names retrieve metadata for.")] List<string> functionNames)
        {
            return await service.GetFunctionsMetadataByNamesAsync(functionNames);
        }

        [McpServerTool, Description("Fetches a list of parameter metadata for a specific stored procedure by its name.")]
        public static async Task<List<ParameterMetadata>> GetStoredProcedureParametersAsync(
            IStoredProcedureFunctionService service,
            [Description("The name of the stored procedure to retrieve parameter metadata for.")] string storedProcedureName)
        {
            return await service.GetStoredProcedureParametersAsync(storedProcedureName);
        }

        [McpServerTool, Description("Fetches a list of parameter metadata for a specific function by its name.")]
        public static async Task<List<ParameterMetadata>> GetFunctionParametersAsync(
            IStoredProcedureFunctionService service,
            [Description("The name of the function to retrieve parameter metadata for.")] string functionName)
        {
            return await service.GetFunctionParametersAsync(functionName);
        }
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using DatabaseMcp.Core.Models;

namespace DatabaseMcp.Core.Interfaces
{
    public interface IStoredProcedureFunctionService
    {
        Task<List<ParameterMetadata>> GetStoredProcedureParametersAsync(string storedProcedureName);
        Task<List<ParameterMetadata>> GetFunctionParametersAsync(string functionName);
        Task<List<ProcedureFunctionMetadata>> GetStoredProceduresMetadataByNamesAsync(List<string> names);
        Task<List<ProcedureFunctionMetadata>> GetFunctionsMetadataByNamesAsync(List<string> names);
    }
}
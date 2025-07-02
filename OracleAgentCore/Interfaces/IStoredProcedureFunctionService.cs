using System.Collections.Generic;
using System.Threading.Tasks;
using OracleAgent.Core.Models;

namespace OracleAgent.Core.Interfaces
{
public interface IStoredProcedureFunctionService
    {
        Task<List<ProcedureFunctionMetadata>> GetAllStoredProceduresAsync();
        Task<List<ProcedureFunctionMetadata>> GetAllFunctionsAsync();        
        Task<List<ParameterMetadata>> GetStoredProcedureParametersAsync(string storedProcedureName);
        Task<List<ParameterMetadata>> GetFunctionParametersAsync(string functionName);
        Task<List<ProcedureFunctionMetadata>> GetStoredProceduresMetadataByNameAsync(List<string> names);
        Task<List<ProcedureFunctionMetadata>> GetFunctionsMetadataByNameAsync(List<string> names);
    }
}
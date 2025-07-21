namespace Hala.DatabaseAgent.Core.Interfaces
{
    /// <summary>
    /// Interface for services that manage stored procedures and functions
    /// </summary>
    public interface IStoredProcedureFunctionService
    {
        /// <summary>
        /// Gets metadata for stored procedures by name
        /// </summary>
        Task<List<Models.StoredProcedureMetadata>> GetStoredProceduresByNameAsync(IEnumerable<string> procedureNames);
        
        /// <summary>
        /// Gets metadata for functions by name
        /// </summary>
        Task<List<Models.FunctionMetadata>> GetFunctionsByNameAsync(IEnumerable<string> functionNames);
        
        /// <summary>
        /// Gets parameters for a stored procedure
        /// </summary>
        Task<List<Models.ParameterMetadata>> GetStoredProcedureParametersAsync(string procedureName);
        
        /// <summary>
        /// Gets parameters for a function
        /// </summary>
        Task<List<Models.ParameterMetadata>> GetFunctionParametersAsync(string functionName);
    }
}

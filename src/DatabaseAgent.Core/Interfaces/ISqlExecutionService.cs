namespace Hala.DatabaseAgent.Core.Interfaces
{
    /// <summary>
    /// Interface for SQL execution service
    /// </summary>
    public interface ISqlExecutionService
    {
        /// <summary>
        /// Executes a SELECT SQL query and returns the result as a list of dictionaries
        /// </summary>
        /// <param name="sqlQuery">The SQL query to execute (must be a SELECT statement)</param>
        /// <param name="parameters">Optional parameters for the query</param>
        /// <returns>A list of dictionaries representing the result rows</returns>
        Task<List<Dictionary<string, object>>> ExecuteSelectQueryAsync(string sqlQuery, IDictionary<string, object>? parameters = null);
    }
}

namespace Hala.DatabaseAgent.Core.Models
{
    /// <summary>
    /// Represents the result of a SQL query
    /// </summary>
    public class SqlQueryResult
    {
        /// <summary>
        /// The column names in the result set
        /// </summary>
        public List<string> ColumnNames { get; set; } = new List<string>();
        
        /// <summary>
        /// The column types in the result set
        /// </summary>
        public List<string> ColumnTypes { get; set; } = new List<string>();
        
        /// <summary>
        /// The rows of data in the result set
        /// </summary>
        public List<List<object?>> Rows { get; set; } = new List<List<object?>>();
        
        /// <summary>
        /// The number of rows affected (for non-query commands)
        /// </summary>
        public int? RowsAffected { get; set; }
        
        /// <summary>
        /// Any error message that occurred during execution
        /// </summary>
        public string? ErrorMessage { get; set; }
        
        /// <summary>
        /// The time it took to execute the query in milliseconds
        /// </summary>
        public long ExecutionTimeMs { get; set; }
    }
}

using Hala.DatabaseAgent.Core.Models;

namespace Hala.DatabaseAgent.Core.Interfaces
{
    /// <summary>
    /// Connection factory interface for database connections
    /// </summary>
    public interface IDbConnectionFactory
    {
        /// <summary>
        /// Creates a new database connection
        /// </summary>
        /// <returns>An open database connection</returns>
        Task<System.Data.Common.DbConnection> CreateConnectionAsync();
        
        /// <summary>
        /// Tests if the connection can be established
        /// </summary>
        /// <returns>True if connection successful, false otherwise</returns>
        Task<bool> TestConnectionAsync();
    }
}

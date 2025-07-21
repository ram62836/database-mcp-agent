using Hala.DatabaseAgent.Core.Models;

namespace Hala.DatabaseAgent.Core.Interfaces
{
    /// <summary>
    /// Interface for database metadata discovery and management
    /// </summary>
    public interface IDatabaseMetadataService
    {
        /// <summary>
        /// Gets all tables in the database
        /// </summary>
        Task<List<TableMetadata>> GetAllUserDefinedTablesAsync();
        
        /// <summary>
        /// Gets the metadata for specific tables
        /// </summary>
        Task<List<TableMetadata>> GetTablesByNameAsync(IEnumerable<string> tableNames);
        
        /// <summary>
        /// Gets tables that contain a specific column name
        /// </summary>
        Task<List<TableMetadata>> GetTablesByColumnNameAsync(string columnName);
        
        /// <summary>
        /// Refreshes the database metadata cache
        /// </summary>
        Task RefreshMetadataCacheAsync();
    }
}

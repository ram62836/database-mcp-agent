using System.Collections.Generic;
using System.Threading.Tasks;
using DatabaseMcp.Core.Models;

namespace DatabaseMcp.Core.Interfaces
{
    public interface IIndexListingService
    {
        /// <summary>  
        /// Lists all indexes for a specified table asynchronously.  
        /// </summary>  
        /// <param name="tableName">The name of the table to list indexes for.</param>  
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of index metadata.</returns>  
        Task<List<IndexMetadata>> ListIndexesAsync(string tableName);

        /// <summary>  
        /// Retrieves the columns associated with a specified index asynchronously.  
        /// </summary>  
        /// <param name="indexName">The name of the index.</param>  
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of column names associated with the index.</returns>  
        Task<List<string>> GetIndexColumnsAsync(string indexName);
    }
}
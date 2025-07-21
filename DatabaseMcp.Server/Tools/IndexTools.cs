using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using DatabaseMcp.Core.Interfaces;
using DatabaseMcp.Core.Models;

namespace DatabaseMcp.Server.Tools
{

    [McpServerToolType()]
    public static class IndexTools
    {
        [McpServerTool, Description("Fetches metadata for all indexes defined on the specified table asynchronously.")]
        public static async Task<List<IndexMetadata>> ListIndexesAsync(
            IIndexListingService service,
            [Description("The name of the table for which to retrieve index metadata.")] string tableName)
        {
            return await service.ListIndexesAsync(tableName);
        }

        [McpServerTool, Description("Fetches the names of columns associated with the specified index asynchronously.")]
        public static async Task<List<string>> GetIndexColumnsAsync(
            IIndexListingService service,
            [Description("The name of the index for which to retrieve associated column names.")] string indexName)
        {
            return await service.GetIndexColumnsAsync(indexName);
        }
    }
}
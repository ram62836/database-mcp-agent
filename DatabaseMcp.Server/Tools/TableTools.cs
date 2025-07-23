using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using DatabaseMcp.Core.Interfaces;
using DatabaseMcp.Core.Models;
using ModelContextProtocol.Server;

namespace DatabaseMcp.Server.Tools
{

    [McpServerToolType()]
    public static class TableTools
    {
        [McpServerTool, Description("Fetches metadata for the given table names. Requires the table names as input.")]
        public static async Task<List<TableMetadata>> GetTablesByNameAsync(
            ITableDiscoveryService service,
            [Description("The names of the tables to retrieve metadata for.")] List<string> tableNames)
        {
            return await service.GetTablesByNameAsync(tableNames);
        }
    }
}
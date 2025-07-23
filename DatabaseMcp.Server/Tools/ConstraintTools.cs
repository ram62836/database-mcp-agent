using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using DatabaseMcp.Core.Interfaces;
using DatabaseMcp.Core.Models;
using ModelContextProtocol.Server;

namespace DatabaseMcp.Server.Tools
{

    [McpServerToolType()]
    public static class ConstraintTools
    {
        [McpServerTool, Description("Retrieves metadata for unique constraints defined on the specified table.")]
        public static async Task<List<ConstraintMetadata>> GetUniqueConstraintsAsync(
            IConstraintGatheringService service,
            [Description("The name of the table for which unique constraints metadata is retrieved.")] string tableName)
        {
            return await service.GetUniqueConstraintsAsync(tableName);
        }

        [McpServerTool, Description("Retrieves metadata for check constraints defined on the specified table.")]
        public static async Task<List<ConstraintMetadata>> GetCheckConstraintsAsync(
            IConstraintGatheringService service,
            [Description("The name of the table for which check constraints metadata is retrieved.")] string tableName)
        {
            return await service.GetCheckConstraintsAsync(tableName);
        }
    }
}
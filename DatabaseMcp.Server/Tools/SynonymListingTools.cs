using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using DatabaseMcp.Core.Interfaces;
using DatabaseMcp.Core.Models;
using ModelContextProtocol.Server;

namespace DatabaseMcp.Server.Tools
{

    [McpServerToolType()]
    public static class SynonymTools
    {
        [McpServerTool, Description("Fetches all synonym metadata from the database asynchronously.")]
        public static async Task<List<SynonymMetadata>> ListIndexesAsync(
            ISynonymListingService service)
        {
            return await service.ListSynonymsAsync();
        }
    }
}
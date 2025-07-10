using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using OracleAgent.Core.Interfaces;
using OracleAgent.Core.Models;

namespace OracleAgent.App.Tools
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
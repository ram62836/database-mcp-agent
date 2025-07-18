using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using DatabaseMcp.Core.Interfaces;
using DatabaseMcp.Core.Models;

namespace DatabaseMcp.Server.Tools
{
    [McpServerToolType()]
    public static class TriggerTools
    {
        [McpServerTool, Description("Fetches metadata for the given trigger names. Requires the trigger names as input.")]
        public static async Task<List<TriggerMetadata>> GetTriggersByNameAsync(
            ITriggerService service,
            [Description("The names of the triggers to retrieve metadata for.")] List<string> triggerNames)
        {
            return await service.GetTriggersByNameAsync(triggerNames);
        }
    }
}

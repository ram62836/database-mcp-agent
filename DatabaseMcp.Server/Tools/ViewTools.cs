using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using DatabaseMcp.Core.Interfaces;
using DatabaseMcp.Core.Models;

namespace DatabaseMcp.Server.Tools
{

    [McpServerToolType()]
    public static class ViewTools
    {

        [McpServerTool, Description("Fetches metadata for a the views by their names asynchronously.")]
        public static async Task<List<ViewMetadata>> GetViewDefinitionAsync(
            IViewEnumerationService service,
            [Description("The names of the views to retrieve metadata for.")] List<string> viewNames)
        {
            return await service.GetViewsDefinitionAsync(viewNames);
        }
    }
}
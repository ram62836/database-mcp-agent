using System.ComponentModel;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using DatabaseMcp.Core.Interfaces;

namespace DatabaseMcp.Server.Tools
{
    [McpServerToolType()]
    public static class RawSqlTool
    {
        [McpServerTool, Description("Executes raw sql select statements.")]
        public static async Task<string> ExecuteRawSelectAsync(
            IRawSqlService service,
            [Description("Raw select statement string.")] string selectStatement)
        {
            return await service.ExecuteRawSelectAsync(selectStatement);
        }
    }
}

using System.ComponentModel;
using System.Threading.Tasks;
using DatabaseMcp.Core.Interfaces;
using ModelContextProtocol.Server;

namespace DatabaseMcp.Server.Tools
{
    [McpServerToolType()]
    public static class PackageTools
    {
        [McpServerTool, Description("Fetches the package definition from the database for the specified package.")]
        public static async Task<string> GetPackageDefinitionAsync(
            IPackageService packageTool,
            [Description("The name of the package to retrieve definition for.")] string packageName)
        {
            return await packageTool.GetPackageDefinitionAsync(packageName);
        }

        [McpServerTool, Description("Fetches the package body implementation from the database for the specified package.")]
        public static async Task<string> GetPackageBodyAsync(
            IPackageService packageTool,
            [Description("The name of the package to retrieve body for.")] string packageName)
        {
            return await packageTool.GetPackageBodyAsync(packageName);
        }
    }
}

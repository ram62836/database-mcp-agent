using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using DatabaseMcp.Core.Interfaces;
using DatabaseMcp.Core.Models;
using ModelContextProtocol.Server;

namespace DatabaseMcp.Server.Tools
{

    [McpServerToolType()]
    public static class KeyIdentificationTools
    {
        [McpServerTool, Description("Retrieves the primary key metadata for the specified table, including column names and constraints.")]
        public static async Task<List<KeyMetadata>> GetPrimaryKeysAsync(
            IKeyIdentificationService service,
            [Description("The name of the table for which to retrieve primary key metadata.")] string tableName)
        {
            return await service.GetPrimaryKeysAsync(tableName);
        }

        [McpServerTool, Description("Retrieves the foreign key metadata for the specified table, including column names and constraints.")]
        public static async Task<List<KeyMetadata>> GetForeignKeysAsync(
            IKeyIdentificationService service,
            [Description("The name of the table for which to retrieve foreign key metadata.")] string tableName)
        {
            return await service.GetForeignKeysAsync(tableName);
        }

        [McpServerTool, Description("Retrieves all foreign key relationships in the database, mapping table names to their related tables.")]
        public static async Task<Dictionary<string, List<string>>> GetForeignKeyRelationshipsAsync(IKeyIdentificationService service)
        {
            return await service.GetForeignKeyRelationshipsAsync();
        }
    }
}
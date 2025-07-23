using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using DatabaseMcp.Core.Interfaces;
using DatabaseMcp.Core.Models;
using ModelContextProtocol.Server;

namespace DatabaseMcp.Server.Tools
{

    [McpServerToolType()]
    public static class ObjectRelationshipTool
    {
        [McpServerTool, Description("Fetches and returns metadata about Oracle database objects that the specified object depends on or references. This includes details about the relationships between the objects.")]
        public static async Task<List<ObjectRelationshipMetadata>> GetObjectsRelationshipsAsync(
            IObjectRelationshipService service,
            [Description("The name of the database object to find for relationships.")] string objectName,
            [Description("The type of the database object to find for relationships.")] string objectType)
        {
            return await service.GetReferenceObjects(objectName, objectType);
        }
    }
}
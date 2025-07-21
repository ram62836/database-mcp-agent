using System.Collections.Generic;
using System.Threading.Tasks;
using DatabaseMcp.Core.Models;

namespace DatabaseMcp.Core.Interfaces
{
    public interface IObjectRelationshipService
    {
        Task<List<ObjectRelationshipMetadata>> GetReferenceObjects(string objectName, string objectType);
    }
}
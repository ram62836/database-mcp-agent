using System.Collections.Generic;
using System.Threading.Tasks;
using OracleAgent.Core.Models;

namespace OracleAgent.Core.Interfaces
{
    public interface IObjectRelationshipService
    {        
        Task<List<ObjectRelationshipMetadata>> GetReferenceObjects(string objectName, string objectType);
    }
}
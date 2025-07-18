using System.Collections.Generic;
using System.Threading.Tasks;
using DatabaseMcp.Core.Models;

namespace DatabaseMcp.Core.Interfaces
{
    public interface IKeyIdentificationService
    {
        Task<List<KeyMetadata>> GetPrimaryKeysAsync(string tableName);
        Task<List<KeyMetadata>> GetForeignKeysAsync(string tableName);
        Task<Dictionary<string, List<string>>> GetForeignKeyRelationshipsAsync();
    }
}
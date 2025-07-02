using System.Collections.Generic;
using System.Threading.Tasks;
using OracleAgent.Core.Models;

namespace OracleAgent.Core.Interfaces
{
    public interface IKeyIdentificationService
    {
        Task<List<KeyMetadata>> GetPrimaryKeysAsync(string tableName);
        Task<List<KeyMetadata>> GetForeignKeysAsync(string tableName);
        Task<Dictionary<string, List<string>>> GetForeignKeyRelationshipsAsync();
    }
}
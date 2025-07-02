using System.Collections.Generic;
using System.Threading.Tasks;
using OracleAgent.Core.Models;

namespace OracleAgent.Core.Interfaces
{
    public interface IConstraintGatheringService
    {
        Task<List<ConstraintMetadata>> GetUniqueConstraintsAsync(string tableName);
        Task<List<ConstraintMetadata>> GetCheckConstraintsAsync(string tableName);
    }
}
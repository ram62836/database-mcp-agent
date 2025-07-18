using System.Collections.Generic;
using System.Threading.Tasks;
using DatabaseMcp.Core.Models;

namespace DatabaseMcp.Core.Interfaces
{
    public interface IConstraintGatheringService
    {
        Task<List<ConstraintMetadata>> GetUniqueConstraintsAsync(string tableName);
        Task<List<ConstraintMetadata>> GetCheckConstraintsAsync(string tableName);
    }
}
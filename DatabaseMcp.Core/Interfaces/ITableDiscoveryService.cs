using System.Collections.Generic;
using System.Threading.Tasks;
using DatabaseMcp.Core.Models;

namespace DatabaseMcp.Core.Interfaces
{
    public interface ITableDiscoveryService
    {
        Task<List<TableMetadata>> GetAllUserDefinedTablesAsync();

        Task<List<TableMetadata>> GetTablesByNameAsync(List<string> tableNames);
    }
}
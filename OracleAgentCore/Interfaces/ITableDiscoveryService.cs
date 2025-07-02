using System.Collections.Generic;
using System.Threading.Tasks;
using OracleAgent.Core.Models;

namespace OracleAgent.Core.Interfaces
{
    public interface ITableDiscoveryService
    {
        Task<List<TableMetadata>> GetAllUserDefinedTablesAsync();

        Task<List<TableMetadata>> GetTablesByNameAsync(List<string> tableNames);
    }
}
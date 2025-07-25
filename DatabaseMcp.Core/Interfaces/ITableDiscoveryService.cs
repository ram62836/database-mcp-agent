using System.Collections.Generic;
using System.Threading.Tasks;
using DatabaseMcp.Core.Models;

namespace DatabaseMcp.Core.Interfaces
{
    public interface ITableDiscoveryService
    {
        Task<List<TableMetadata>> GetTablesMetadataByNamesAsync(List<string> tableNames);
    }
}
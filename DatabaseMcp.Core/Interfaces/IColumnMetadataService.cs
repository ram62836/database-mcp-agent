using System.Collections.Generic;
using System.Threading.Tasks;
using DatabaseMcp.Core.Models;

namespace DatabaseMcp.Core.Interfaces
{
    public interface IColumnMetadataService
    {
        Task<List<ColumnMetadata>> GetColumnMetadataAsync(string tableName);
        Task<List<string>> GetColumnNamesAsync(string tableName);
        Task<List<ColumnMetadata>> GetDataTypesAsync(string tableName);
        Task<List<ColumnMetadata>> GetNullabilityAsync(string tableName);
        Task<List<ColumnMetadata>> GetDefaultValuesAsync(string tableName);
        Task<List<string>> GetTablesByColumnNameAsync(string columnNamePattern);
    }
}
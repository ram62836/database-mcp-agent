using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using OracleAgent.Core.Interfaces;
using OracleAgent.Core.Models;

namespace OracleAgent.App.Tools
{

    [McpServerToolType()]
    public static class ColumnTools
    {
        [McpServerTool, Description("Fetches detailed metadata for all columns in the specified table, including name, data type, nullability, default value, and ordinal position.")]
        public static async Task<List<ColumnMetadata>> GetColumnMetadata(
            IColumnMetadataService service,
            [Description("The name of the table for which column metadata is to be retrieved.")] string tableName)
        {
            return await service.GetColumnMetadataAsync(tableName);
        }

        [McpServerTool, Description("Fetches the names of all columns in the specified table.")]
        public static async Task<List<string>> GetColumnNames(
            IColumnMetadataService service,
            [Description("The name of the table for which column names are to be retrieved.")] string tableName)
        {
            return await service.GetColumnNamesAsync(tableName);
        }

        [McpServerTool, Description("Fetches the data types of all columns in the specified table.")]
        public static async Task<List<ColumnMetadata>> GetDataTypes(
            IColumnMetadataService service,
            [Description("The name of the table for which column data types are to be retrieved.")] string tableName)
        {
            return await service.GetDataTypesAsync(tableName);
        }

        [McpServerTool, Description("Fetches information about which columns in the specified table are nullable.")]
        public static async Task<List<ColumnMetadata>> GetNullability(
            IColumnMetadataService service,
            [Description("The name of the table for which column nullability information is to be retrieved.")] string tableName)
        {
            return await service.GetNullabilityAsync(tableName);
        }

        [McpServerTool, Description("Fetches the default values of all columns in the specified table.")]
        public static async Task<List<ColumnMetadata>> GetDefaultValues(
            IColumnMetadataService service,
            [Description("The name of the table for which column default values are to be retrieved.")] string tableName)
        {
            return await service.GetDefaultValuesAsync(tableName);
        }

        [McpServerTool, Description("Find tables that contain the specified column name.")]
        public static async Task<List<string>> GetTablesByColumnNameAsync(
            IColumnMetadataService service,
            [Description("Usign the column name, identify all tables that include the given column name.")] string columnName)
        {
            return await service.GetTablesByColumnNameAsync(columnName);
        }
    }
}
using DatabaseMcp.Core.Models;

namespace DatabaseMcp.Core.Tests
{
    public static class TestDataSeeder
    {
        public static List<ColumnMetadata> GetSampleColumnMetadataList()
        {
            return [
            new ColumnMetadata { Name = "ID", DataType = "NUMBER", IsNullable = false, DefaultValue = null, OrdinalPosition = 1 },
            new ColumnMetadata { Name = "NAME", DataType = "VARCHAR2", IsNullable = true, DefaultValue = "'N/A'", OrdinalPosition = 2 }
        ];
        }
    }
}

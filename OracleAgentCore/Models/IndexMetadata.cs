using System.Collections.Generic;

namespace OracleAgent.Core.Models
{
    public class IndexMetadata
    {
        public string IndexName { get; set; }
        public string TableName { get; set; }
        public bool IsUnique { get; set; }
        public bool IsClustered { get; set; }
        public List<string> ColumnNames { get; set; }

        public IndexMetadata()
        {
            ColumnNames = [];
        }
    }
}
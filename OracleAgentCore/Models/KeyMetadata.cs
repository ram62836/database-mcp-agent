using System.Collections.Generic;

namespace OracleAgent.Core.Models
{
    public class KeyMetadata
    {
        public string ColumnName { get; set; }
        public string ConstraintName { get; set; }

        public string KeyType { get; set; }
        public string ReferencedConstraintName { get; set; }
        

        public KeyMetadata()
        {
            ColumnName = string.Empty;
            ConstraintName = string.Empty;
            KeyType = string.Empty;
            ReferencedConstraintName = string.Empty;
        }
    }
}
using System.Collections.Generic;

namespace OracleAgent.Core.Models
{
    public class ConstraintMetadata
    {
        public string ConstraintName { get; set; }
        public string ConstraintType { get; set; }
        public string ColumnName { get; set; }
        public string SearchCondition { get; set; }        
    }
}
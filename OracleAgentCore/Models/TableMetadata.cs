using System.Collections.Generic;

namespace OracleAgent.Core.Models
{
    public class TableMetadata
    {
        public string TableName { get; set; }
        public string Schema { get; set; }
        public string Description { get; set; }
        public string Definition { get; set; }  
        public List<ColumnMetadata> Columns { get; set; }
        public List<KeyMetadata> Keys { get; set; }
        public List<IndexMetadata> Indexes { get; set; }
        public List<ConstraintMetadata> Constraints { get; set; }
        public List<ViewMetadata> Views { get; set; }
        public List<SynonymMetadata> Synonyms { get; set; }
        public List<ProcedureFunctionMetadata> StoredProceduresAndFunctions { get; set; }

        public TableMetadata()
        {
            Columns = new List<ColumnMetadata>();
            Keys = new List<KeyMetadata>();
            Indexes = new List<IndexMetadata>();
            Constraints = new List<ConstraintMetadata>();
            Views = new List<ViewMetadata>();
            Synonyms = new List<SynonymMetadata>();
            StoredProceduresAndFunctions = new List<ProcedureFunctionMetadata>();
        }
    }
}
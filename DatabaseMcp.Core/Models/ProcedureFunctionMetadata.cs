using System.Collections.Generic;

namespace DatabaseMcp.Core.Models
{
    public class ProcedureFunctionMetadata
    {
        public string Name { get; set; }
        public string Type { get; set; } // e.g., "StoredProcedure" or "Function"
        public string Schema { get; set; }
        public string ReturnType { get; set; }
        public string Definition { get; set; }

        public List<ParameterMetadata> Parameters { get; set; }

        public ProcedureFunctionMetadata()
        {
            Parameters = [];
        }
    }
}
using System;

namespace OracleAgent.Core.Models
{
    public class ViewMetadata
    {
        public string ViewName { get; set; }
        public string Schema { get; set; }
        public string Definition { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
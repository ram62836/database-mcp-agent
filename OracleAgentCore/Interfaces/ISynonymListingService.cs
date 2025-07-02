using System.Collections.Generic;
using System.Threading.Tasks;
using OracleAgent.Core.Models;

namespace OracleAgent.Core.Interfaces
{
    public interface ISynonymListingService
    {
        Task<List<SynonymMetadata>> ListSynonymsAsync();
    }
}
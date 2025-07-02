using System.Collections.Generic;
using System.Threading.Tasks;
using OracleAgent.Core.Models;

namespace OracleAgent.Core.Interfaces
{
    public interface IViewEnumerationService
    {
        Task<List<ViewMetadata>> GetAllViewsAsync();
        Task<List<ViewMetadata>> GetViewsDefinitionAsync(List<string> viewNames);
    }
}
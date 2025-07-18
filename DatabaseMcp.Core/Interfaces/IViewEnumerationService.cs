using System.Collections.Generic;
using System.Threading.Tasks;
using DatabaseMcp.Core.Models;

namespace DatabaseMcp.Core.Interfaces
{
    public interface IViewEnumerationService
    {
        Task<List<ViewMetadata>> GetAllViewsAsync();
        Task<List<ViewMetadata>> GetViewsDefinitionAsync(List<string> viewNames);
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DatabaseMcp.Core.Interfaces
{
    public interface IPackageService
    {
        Task<string> GetPackageDefinitionAsync(string packageName);
        Task<string> GetPackageBodyAsync(string packageName);
    }
}

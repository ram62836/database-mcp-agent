using System.Collections.Generic;
using System.Threading.Tasks;
using DatabaseMcp.Core.Models;

namespace DatabaseMcp.Core.Interfaces
{
    public interface ITriggerService
    {
        Task<List<TriggerMetadata>> GetTriggersMetadatByNamesAsync(List<string> triggerNames);
    }
}

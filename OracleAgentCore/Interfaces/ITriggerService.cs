using System.Collections.Generic;
using System.Threading.Tasks;
using OracleAgent.Core.Models;

namespace OracleAgent.Core.Interfaces
{
    public interface ITriggerService
    {
        Task<List<TriggerMetadata>> GetAllTriggersAsync();
        Task<List<TriggerMetadata>> GetTriggersByNameAsync(List<string> triggerNames);
    }
}

using System.Threading.Tasks;

namespace OracleAgent.Core.Interfaces
{
    public interface IRawSqlService
    {
        Task<string> ExecuteRawSelectAsync(string rawSelectSql);
    }
}

using System.Threading.Tasks;

namespace DatabaseMcp.Core.Interfaces
{
    public interface IRawSqlService
    {
        Task<string> ExecuteRawSelectAsync(string rawSelectSql);
    }
}

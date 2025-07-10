using System.Data;
using System.Threading.Tasks;

namespace OracleAgent.Core
{
    public interface IDbConnectionFactory
    {
        Task<IDbConnection> CreateConnectionAsync();
    }
}

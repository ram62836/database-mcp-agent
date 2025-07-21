using System.Data;
using System.Threading.Tasks;

namespace DatabaseMcp.Core
{
    public interface IDbConnectionFactory
    {
        Task<IDbConnection> CreateConnectionAsync();
    }
}

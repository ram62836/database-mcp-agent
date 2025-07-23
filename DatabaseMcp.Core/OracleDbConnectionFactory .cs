using System.Data;
using System.Threading.Tasks;
using DatabaseMcp.Core.Services;
using Oracle.ManagedDataAccess.Client;

namespace DatabaseMcp.Core
{
    public class OracleDbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public OracleDbConnectionFactory(DatabaseConnectionService connectionService)
        {
            _connectionString = connectionService.GetOracleConnectionString();
        }

        public async Task<IDbConnection> CreateConnectionAsync()
        {
            OracleConnection connection = new(_connectionString);
            await connection.OpenAsync();
            return connection;
        }
    }
}

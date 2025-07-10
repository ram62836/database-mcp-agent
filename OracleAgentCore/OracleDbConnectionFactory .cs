using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace OracleAgent.Core
{
    public class OracleDbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public OracleDbConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IDbConnection> CreateConnectionAsync()
        {
            var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }
    }
}

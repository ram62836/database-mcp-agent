using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using DatabaseMcp.Core.Services;

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

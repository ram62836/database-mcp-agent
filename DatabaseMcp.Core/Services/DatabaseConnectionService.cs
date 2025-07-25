using System;
using Microsoft.Extensions.Configuration;

namespace DatabaseMcp.Core.Services
{
    public class DatabaseConnectionService
    {
        private readonly IConfiguration _configuration;

        public DatabaseConnectionService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public string GetOracleConnectionString()
        {
            // Try to get the full connection string from environment variable via IConfiguration
            // First try direct environment variable access
            string fullConnectionString = _configuration["OracleConnectionString"];
            return !string.IsNullOrWhiteSpace(fullConnectionString)
                ? fullConnectionString
                : throw new InvalidOperationException(
                "No Oracle connection configuration found. Please provide OracleConnectionString environment variable with full connection string");
        }
    }
}

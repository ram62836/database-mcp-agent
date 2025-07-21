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
            var fullConnectionString = _configuration["OracleConnectionString"];
            if (!string.IsNullOrEmpty(fullConnectionString))
            {
                return fullConnectionString;
            }
            
            // Try with ConnectionStrings section as fallback
            fullConnectionString = _configuration.GetConnectionString("OracleConnectionString");
            if (!string.IsNullOrEmpty(fullConnectionString))
            {
                return fullConnectionString;
            }

            // Build connection string from individual environment variables
            // IConfiguration automatically reads from environment variables with these keys
            var host = _configuration["ORACLE_DATABASE_HOST"];
            var port = _configuration["ORACLE_DATABASE_PORT"] ?? "1521";
            var serviceName = _configuration["ORACLE_DATABASE_SERVICE_NAME"];
            var username = _configuration["ORACLE_DATABASE_USERNAME"];
            var password = _configuration["ORACLE_DATABASE_PASSWORD"];

            if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(serviceName) && 
                !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                return $"Host={host};Port={port};Database={serviceName};User Id={username};Password={password};";
            }

            throw new InvalidOperationException(
                "No Oracle connection configuration found. Please provide either:\n" +
                "1. OracleConnectionString environment variable with full connection string, OR\n" +
                "2. Individual environment variables: ORACLE_DATABASE_HOST, ORACLE_DATABASE_SERVICE_NAME, ORACLE_DATABASE_USERNAME, ORACLE_DATABASE_PASSWORD");
        }
    }
}

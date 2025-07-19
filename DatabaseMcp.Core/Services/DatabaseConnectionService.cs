using System;
using Microsoft.Extensions.Configuration;

namespace DatabaseMcp.Core.Services
{
    public class DatabaseConnectionService
    {
        private readonly IConfiguration _configuration;

        public DatabaseConnectionService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetOracleConnectionString()
        {
            // First try to get the full connection string from environment variable
            var fullConnectionString = _configuration.GetConnectionString("OracleConnection");
            if (!string.IsNullOrEmpty(fullConnectionString))
            {
                return fullConnectionString;
            }

            // Fall back to the legacy appsettings.json DefaultConnection
            var legacyConnectionString = _configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrEmpty(legacyConnectionString))
            {
                return legacyConnectionString;
            }

            // Build connection string from individual environment variables
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
                "1. ConnectionStrings__OracleConnection environment variable with full connection string, OR\n" +
                "2. Individual environment variables: ORACLE_DATABASE_HOST, ORACLE_DATABASE_SERVICE_NAME, ORACLE_DATABASE_USERNAME, ORACLE_DATABASE_PASSWORD, OR\n" +
                "3. ConnectionStrings:DefaultConnection in appsettings.json");
        }

        public int GetCacheExpirationMinutes()
        {
            var cacheExpiration = _configuration["DatabaseMcp:CacheExpirationMinutes"];
            return string.IsNullOrEmpty(cacheExpiration) ? 30 : int.Parse(cacheExpiration);
        }

        public int GetMaxConnectionRetries()
        {
            var maxRetries = _configuration["DatabaseMcp:MaxConnectionRetries"];
            return string.IsNullOrEmpty(maxRetries) ? 3 : int.Parse(maxRetries);
        }
    }
}

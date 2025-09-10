using System;
using System.Data;
using System.Threading.Tasks;
using DatabaseMcp.Core.Interfaces;

namespace DatabaseMcp.Core.Services
{
    public class PackageService : IPackageService
    {
        private readonly IRawSqlService _rawSqlService;
        private readonly IObjectRelationshipService _objectRelationshipService;
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly string _owner;

        public PackageService(IRawSqlService rawSqlService, IObjectRelationshipService objectRelationshipService, IDbConnectionFactory connectionFactory)
        {
            _rawSqlService = rawSqlService;
            _objectRelationshipService = objectRelationshipService;
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _owner = Environment.GetEnvironmentVariable("SchemaOwner");
        }

        public async Task<string> GetPackageDefinitionAsync(string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
                throw new ArgumentException("Package name cannot be null or empty", nameof(packageName));

            using (IDbConnection connection = await _connectionFactory.CreateConnectionAsync())
            {
                string sql = string.IsNullOrEmpty(_owner)
                    ? @"SELECT TEXT FROM ALL_SOURCE WHERE NAME = :PackageName AND TYPE = 'PACKAGE' ORDER BY LINE"
                    : @"SELECT TEXT FROM ALL_SOURCE WHERE NAME = :PackageName AND OWNER = :Owner AND TYPE = 'PACKAGE' ORDER BY LINE";

                using IDbCommand command = connection.CreateCommand();
                command.CommandText = sql;
                
                IDbDataParameter nameParam = command.CreateParameter();
                nameParam.ParameterName = "PackageName";
                nameParam.Value = packageName.ToUpper();
                _ = command.Parameters.Add(nameParam);

                if (!string.IsNullOrEmpty(_owner))
                {
                    IDbDataParameter ownerParam = command.CreateParameter();
                    ownerParam.ParameterName = "Owner";
                    ownerParam.Value = _owner;
                    _ = command.Parameters.Add(ownerParam);
                }

                using IDataReader reader = command.ExecuteReader();
                var result = new System.Text.StringBuilder();
                while (reader.Read())
                {
                    var text = reader["TEXT"];
                    if (text != null && text != System.DBNull.Value)
                    {
                        result.Append(text.ToString());
                    }
                }
                return result.ToString();
            }
        }

        public async Task<string> GetPackageBodyAsync(string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
                throw new ArgumentException("Package name cannot be null or empty", nameof(packageName));

            using (IDbConnection connection = await _connectionFactory.CreateConnectionAsync())
            {
                string sql = string.IsNullOrEmpty(_owner)
                    ? @"SELECT TEXT FROM ALL_SOURCE WHERE NAME = :PackageName AND TYPE = 'PACKAGE BODY' ORDER BY LINE"
                    : @"SELECT TEXT FROM ALL_SOURCE WHERE NAME = :PackageName AND OWNER = :Owner AND TYPE = 'PACKAGE BODY' ORDER BY LINE";

                using IDbCommand command = connection.CreateCommand();
                command.CommandText = sql;
                
                IDbDataParameter nameParam = command.CreateParameter();
                nameParam.ParameterName = "PackageName";
                nameParam.Value = packageName.ToUpper();
                _ = command.Parameters.Add(nameParam);

                if (!string.IsNullOrEmpty(_owner))
                {
                    IDbDataParameter ownerParam = command.CreateParameter();
                    ownerParam.ParameterName = "Owner";
                    ownerParam.Value = _owner;
                    _ = command.Parameters.Add(ownerParam);
                }

                using IDataReader reader = command.ExecuteReader();
                var result = new System.Text.StringBuilder();
                while (reader.Read())
                {
                    var text = reader["TEXT"];
                    if (text != null && text != System.DBNull.Value)
                    {
                        result.Append(text.ToString());
                    }
                }
                return result.ToString();
            }
        }
    }
}

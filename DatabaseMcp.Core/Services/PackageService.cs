using System.Collections.Generic;
using System.Threading.Tasks;

namespace DatabaseMcp.Core.Services
{
    using DatabaseMcp.Core.Interfaces;

    public class PackageService : IPackageService
    {
        private readonly IRawSqlService _rawSqlService;
        private readonly IObjectRelationshipService _objectRelationshipService;

        public PackageService(IRawSqlService rawSqlService, IObjectRelationshipService objectRelationshipService)
        {
            _rawSqlService = rawSqlService;
            _objectRelationshipService = objectRelationshipService;
        }

        public async Task<string> GetPackageDefinitionAsync(string packageName)
        {
            var sql = $@"SELECT TEXT FROM ALL_SOURCE WHERE NAME = '{packageName.ToUpper()}' AND TYPE = 'PACKAGE' ORDER BY LINE";
            var result = await _rawSqlService.ExecuteRawSelectAsync(sql);
            return result;
        }

        public async Task<string> GetPackageBodyAsync(string packageName)
        {
            var sql = $@"SELECT TEXT FROM ALL_SOURCE WHERE NAME = '{packageName.ToUpper()}' AND TYPE = 'PACKAGE BODY' ORDER BY LINE";
            var result = await _rawSqlService.ExecuteRawSelectAsync(sql);
            return result;
        }
    }
}

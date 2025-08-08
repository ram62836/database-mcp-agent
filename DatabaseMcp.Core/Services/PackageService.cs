using System.Threading.Tasks;
using DatabaseMcp.Core.Interfaces;

namespace DatabaseMcp.Core.Services
{
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
            string sql = $@"SELECT TEXT FROM ALL_SOURCE WHERE NAME = '{packageName.ToUpper()}' AND TYPE = 'PACKAGE' ORDER BY LINE";
            string result = await _rawSqlService.ExecuteRawSelectAsync(sql);
            return result;
        }

        public async Task<string> GetPackageBodyAsync(string packageName)
        {
            string sql = $@"SELECT TEXT FROM ALL_SOURCE WHERE NAME = '{packageName.ToUpper()}' AND TYPE = 'PACKAGE BODY' ORDER BY LINE";
            string result = await _rawSqlService.ExecuteRawSelectAsync(sql);
            return result;
        }
    }
}

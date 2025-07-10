using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using OracleAgent.Core;
using OracleAgent.Core.Interfaces;

namespace OracleAgent.App.Tools
{
    [McpServerToolType()]
    public class DBMetadataRefreshTool
    {
        private readonly IStoredProcedureFunctionService _storedProcedureFunctionsService;
        private readonly ITableDiscoveryService _tableDiscoveryService;
        private readonly ITriggerService _triggerService;
        private readonly IViewEnumerationService _viewsService;

        public DBMetadataRefreshTool(
            IStoredProcedureFunctionService storedProcedureFunctionsService,
            ITableDiscoveryService tableDiscoveryService,
            ITriggerService triggerService,
            IViewEnumerationService viewsService)
        {
            _storedProcedureFunctionsService = storedProcedureFunctionsService;
            _tableDiscoveryService = tableDiscoveryService;
            _triggerService = triggerService;
            _viewsService = viewsService;
        }

        [McpServerTool, Description("Refresh DB metadata cache, which is stored in JSON files for StoredProcedures/Functions/Tables/Triggers/Views.")]
        public async Task RefreshFullDBMetadataAsync()
        {
            _ = await _storedProcedureFunctionsService.GetAllStoredProceduresAsync();
            _ = await _storedProcedureFunctionsService.GetAllFunctionsAsync();
            _ = await _tableDiscoveryService.GetAllUserDefinedTablesAsync();
            _ = await _triggerService.GetAllTriggersAsync();
            _ = await _viewsService.GetAllViewsAsync();
        }

        [McpServerTool, Description("Refresh DB metadata cache for StoredProcedures, which is stored in JSON files.")]
        public async Task RefreshStoredProceduresMetadataAsync()
        {
            File.Delete(AppConstants.ProceduresMetadatJsonFile);
            _ = await _storedProcedureFunctionsService.GetAllStoredProceduresAsync();
        }

        [McpServerTool, Description("Refresh DB metadata cache for Functions, which is stored in JSON files.")]
        public async Task RefreshFunctionsMetadataAsync()
        {
            File.Delete(AppConstants.FunctionsMetadataJsonFile);
            _ = await _storedProcedureFunctionsService.GetAllFunctionsAsync();
        }

        [McpServerTool, Description("Refresh DB metadata cache for Tables, which is stored in JSON files.")]
        public async Task RefreshTablesMetadataAsync()
        {
            File.Delete(AppConstants.TablesMetadatJsonFile);
            _ = await _tableDiscoveryService.GetAllUserDefinedTablesAsync();
        }

        [McpServerTool, Description("Refresh DB metadata cache for Triggers, which is stored in JSON files.")]
        public async Task RefreshTriggersMetadataAsync()
        {
            File.Delete(AppConstants.TriggersMetadataJsonFile);
            _ = await _triggerService.GetAllTriggersAsync();
        }

        [McpServerTool, Description("Refresh DB metadata cache for Views, which is stored in JSON files.")]
        public async Task RefreshViewsMetadataAsync()
        {
            File.Delete(AppConstants.ViewsMetadatJsonFile);
            _ = await _viewsService.GetAllViewsAsync();
        }
    }
}

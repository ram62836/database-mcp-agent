using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DatabaseMcp.Core.Interfaces;
using DatabaseMcp.Core.Models;
using ModelContextProtocol.Server;

namespace DatabaseMcp.Server.Tools
{
    [McpServerToolType()]
    public static class ObjectDependencyTools
    {
        [McpServerTool, Description("Analyzes dependencies for a given object returns the dependent objects definitions.")]
        public static async Task<List<object>> DependentObjectsAnalysis(
            IObjectRelationshipService relationshipService,
            IStoredProcedureFunctionService spFunctionService,
            ITriggerService triggerService,
            [Description("The name of the object to analyze.")] string objectName,
            [Description("The type of the object to analyze (e.g., TABLE, VIEW, PROCEDURE, FUNCTION).")] string objectType,
            CancellationToken cancellationToken)
        {
            List<string> DMLSummaries = [];
            // 1. Get dependent objects
            List<ObjectRelationshipMetadata> dependencies = await relationshipService.GetReferenceObjects(objectName, objectType);
            // 2. Retrieve definitions for each dependent object
            List<string> procedureNames = dependencies
                .Where(d => d.ObjectType.Equals("PROCEDURE", StringComparison.OrdinalIgnoreCase))
                .Select(d => d.ObjectName)
                .ToList();

            List<string> functionNames = dependencies
                .Where(d => d.ObjectType.Equals("FUNCTION", StringComparison.OrdinalIgnoreCase))
                .Select(d => d.ObjectName)
                .ToList();

            List<string> triggerNames = dependencies
                .Where(d => d.ObjectType.Equals("TRIGGER", StringComparison.OrdinalIgnoreCase))
                .Select(d => d.ObjectName)
                .ToList();

            List<ProcedureFunctionMetadata> proceduresAndFunctions = [];
            // Token limitation - need to perform in-session analysis to overcome the token limitation
            foreach (string name in procedureNames.Take(5))
            {
                List<ProcedureFunctionMetadata> result = await spFunctionService.GetStoredProceduresMetadataByNamesAsync([name]);
                if (result != null && result.Count > 0)
                {
                    proceduresAndFunctions.Add(result.First());
                }
            }

            // Token limitation - need to perform in-session analysis to overcome the token limitation
            foreach (string name in functionNames.Take(5))
            {
                List<ProcedureFunctionMetadata> result = await spFunctionService.GetFunctionsMetadataByNamesAsync([name]);
                if (result != null && result.Count > 0)
                {
                    proceduresAndFunctions.Add(result.First());
                }
            }

            List<TriggerMetadata> triggersMetadata = [];
            foreach (string name in triggerNames.Take(5))
            {
                List<TriggerMetadata> result = await triggerService.GetTriggersMetadatByNamesAsync([name]);
                if (result != null && result.Count > 0)
                {
                    triggersMetadata.Add(result.First());
                }
            }

            List<dynamic> objects = [.. proceduresAndFunctions, .. triggersMetadata];
            return objects;
        }
    }
}

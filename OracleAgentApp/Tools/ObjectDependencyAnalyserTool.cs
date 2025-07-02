using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;
using OracleAgent.Core.Interfaces;
using OracleAgent.Core.Models;

namespace OracleAgent.App.Tools
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
            var DMLSummaries = new List<string>();
            // 1. Get dependent objects
            var dependencies = await relationshipService.GetReferenceObjects(objectName, objectType);
            // 2. Retrieve definitions for each dependent object
            var procedureNames = dependencies
                .Where(d => d.ObjectType.Equals("PROCEDURE", StringComparison.OrdinalIgnoreCase))
                .Select(d => d.ObjectName)
                .ToList();

            var functionNames = dependencies
                .Where(d => d.ObjectType.Equals("FUNCTION", StringComparison.OrdinalIgnoreCase))
                .Select(d => d.ObjectName)
                .ToList();

            var triggerNames = dependencies
                .Where(d => d.ObjectType.Equals("TRIGGER", StringComparison.OrdinalIgnoreCase))
                .Select(d => d.ObjectName)
                .ToList();

            var proceduresAndFunctions = new List<ProcedureFunctionMetadata>();
            // Token limitation - need to perform in-session analysis to overcome the token limitation
            foreach (var name in procedureNames.Take(5))
            {
                var result = await spFunctionService.GetStoredProceduresMetadataByNameAsync(new List<string>() { name });
                if (result != null && result.Count > 0)
                {
                    proceduresAndFunctions.Add(result.First());
                }
            }

            // Token limitation - need to perform in-session analysis to overcome the token limitation
            foreach (var name in functionNames.Take(5))
            {
                var result = await spFunctionService.GetFunctionsMetadataByNameAsync(new List<string>() { name });
                if (result != null && result.Count > 0)
                {
                    proceduresAndFunctions.Add(result.First());
                }
            }

            var triggersMetadata = new List<TriggerMetadata>();
            foreach (var name in triggerNames.Take(5))
            {
                var result = await triggerService.GetTriggersByNameAsync(new List<string>() { name });
                if (result != null && result.Count > 0)
                {
                    triggersMetadata.Add(result.First());
                }
            }

            var objects = new List<dynamic>();
            objects.AddRange(proceduresAndFunctions);
            objects.AddRange(triggersMetadata);
            return objects;
        }
    }
}

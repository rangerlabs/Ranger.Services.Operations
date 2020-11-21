using System;
using System.Collections.Generic;
using System.Linq;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Operations.Commands
{
    [MessageNamespaceAttribute("operations")]
    public class BulkDeleteGeofencesSagaInitializer : SagaInitializer, ICommand
    {
        public BulkDeleteGeofencesSagaInitializer(
            bool frontendRequest,
            string commandingUserEmailOrTokenPrefix,
            string tenantid,
            Guid projectId,
            IEnumerable<string> externalIds)

        {
            if (string.IsNullOrWhiteSpace(commandingUserEmailOrTokenPrefix))
            {
                throw new System.ArgumentException($"{nameof(commandingUserEmailOrTokenPrefix)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(tenantid))
            {
                throw new System.ArgumentException($"{nameof(tenantid)} was null or whitespace");
            }
            if (externalIds is null || externalIds.Count() is 0)
            {
                throw new System.ArgumentException($"{nameof(externalIds)} must not be default or empty");
            }
            FrontendRequest = frontendRequest;
            CommandingUserEmailOrTokenPrefix = commandingUserEmailOrTokenPrefix;
            Tenantid = tenantid;
            ExternalIds = externalIds;
            ProjectId = projectId;
        }

        public bool FrontendRequest { get; }
        public string CommandingUserEmailOrTokenPrefix { get; }
        public string Tenantid { get; }
        public IEnumerable<string> ExternalIds { get; }
        public Guid ProjectId { get; }
    }
}
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
            string tenantId,
            Guid projectId,
            IEnumerable<string> externalIds)

        {
            if (string.IsNullOrWhiteSpace(commandingUserEmailOrTokenPrefix))
            {
                throw new System.ArgumentException($"{nameof(commandingUserEmailOrTokenPrefix)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new System.ArgumentException($"{nameof(tenantId)} was null or whitespace");
            }
            if (externalIds is null || !externalIds.Any())
            {
                throw new System.ArgumentException($"{nameof(externalIds)} must not be null or empty");
            }
            FrontendRequest = frontendRequest;
            CommandingUserEmailOrTokenPrefix = commandingUserEmailOrTokenPrefix;
            TenantId = tenantId;
            ExternalIds = externalIds;
            ProjectId = projectId;
        }

        public bool FrontendRequest { get; }
        public string CommandingUserEmailOrTokenPrefix { get; }
        public IEnumerable<string> ExternalIds { get; }
        public Guid ProjectId { get; }
    }
}
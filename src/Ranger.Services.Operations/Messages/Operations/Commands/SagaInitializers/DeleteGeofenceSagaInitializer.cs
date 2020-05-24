using System;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Operations
{
    [MessageNamespaceAttribute("operations")]
    public class DeleteGeofenceSagaInitializer : SagaInitializer, ICommand
    {
        public DeleteGeofenceSagaInitializer(bool frontendRequest, string commandingUserEmailOrTokenPrefix, string tenantId, string externalId, Guid projectId)
        {
            if (string.IsNullOrWhiteSpace(commandingUserEmailOrTokenPrefix))
            {
                throw new System.ArgumentException($"{nameof(commandingUserEmailOrTokenPrefix)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new System.ArgumentException($"{nameof(tenantId)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(externalId))
            {
                throw new System.ArgumentException($"{nameof(externalId)} was null or whitespace");
            }


            this.FrontendRequest = frontendRequest;
            this.CommandingUserEmailOrTokenPrefix = commandingUserEmailOrTokenPrefix;

            TenantId = tenantId;
            this.ExternalId = externalId;
            this.ProjectId = projectId;
        }
        public bool FrontendRequest { get; }
        public string CommandingUserEmailOrTokenPrefix { get; }
        public string ExternalId { get; }
        public Guid ProjectId { get; }
    }
}
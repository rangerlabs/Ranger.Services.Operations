using System;
using Ranger.Common;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Subscriptions
{
    [MessageNamespace("subscriptions")]
    public class ResourceCountIncremented : IEvent
    {
        public string TenantId { get; }
        public ResourceEnum Resource { get; }
        public int NewCount { get; }

        public ResourceCountIncremented(string tenantId, ResourceEnum resource, int newCount)
        {
            if (String.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} was null or whitespace.");
            }
            if (newCount < 1)
            {
                throw new ArgumentException($"{nameof(newCount)} was less than 1.");
            }
            this.TenantId = tenantId;
            this.Resource = resource;
            this.NewCount = newCount;
        }
    }
}
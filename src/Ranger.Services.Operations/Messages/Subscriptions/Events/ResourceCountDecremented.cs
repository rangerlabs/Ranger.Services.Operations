using System;
using Ranger.Common;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Subscriptions
{
    [MessageNamespace("subscriptions")]
    public class ResourceCountDecremented : IEvent
    {
        public string TenantId { get; }
        public ResourceEnum Resource { get; }
        public int NewCount { get; }

        public ResourceCountDecremented(string tenantId, ResourceEnum resource, int newCount)
        {
            if (String.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} was null or whitespace.");
            }
            if (newCount < 0)
            {
                throw new ArgumentException($"{nameof(newCount)} was less than 0.");
            }
            this.TenantId = tenantId;
            this.Resource = resource;
            this.NewCount = newCount;
        }
    }
}
using System;
using Ranger.Common;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Subscriptions
{
    [MessageNamespace("subscriptions")]
    public class IncrementResourceCount : ICommand
    {
        public string TenantId { get; }
        public ResourceEnum Resource { get; }

        public IncrementResourceCount(string tenantId, ResourceEnum resource)
        {
            if (String.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} was null or whitespace");
            }
            this.TenantId = tenantId;
            this.Resource = resource;
        }
    }
}
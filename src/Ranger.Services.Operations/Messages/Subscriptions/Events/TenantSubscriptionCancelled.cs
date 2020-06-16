using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Subscriptions.Events
{
    [MessageNamespace("subscriptions")]
    public class TenantSubscriptionCancelled : IEvent
    {
        public string TenantId { get; }

        public TenantSubscriptionCancelled(string tenantId)
        {
            this.TenantId = tenantId;
        }
    }
}
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Subscriptions.Commands
{
    [MessageNamespace("subscriptions")]
    public class CancelTenantSubscription : ICommand
    {
        public string TenantId { get; }
        public string OrganizationName { get; }
        public CancelTenantSubscription(string tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new System.ArgumentException($"'{nameof(tenantId)}' cannot be null or whitespace", nameof(tenantId));
            }

            this.TenantId = tenantId;
        }
    }
}
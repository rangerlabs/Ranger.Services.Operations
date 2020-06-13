using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Subscriptions.Commands
{
    [MessageNamespace("subscriptions")]
    public class UpdateTenantSubscriptionOrganization : ICommand
    {
        public string TenantId { get; }
        public string OrganizationName { get; }
        public UpdateTenantSubscriptionOrganization(string tenantId, string organizationName)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new System.ArgumentException($"'{nameof(tenantId)}' cannot be null or whitespace", nameof(tenantId));
            }

            if (string.IsNullOrWhiteSpace(organizationName))
            {
                throw new System.ArgumentException($"'{nameof(organizationName)}' cannot be null or whitespace", nameof(organizationName));
            }

            this.TenantId = tenantId;
            this.OrganizationName = organizationName;
        }
    }
}
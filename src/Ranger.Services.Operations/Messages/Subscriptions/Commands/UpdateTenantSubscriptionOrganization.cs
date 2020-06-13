using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Subscriptions.Commands
{
    [MessageNamespace("subscriptions")]
    public class UpdateTenantSubscriptionOrganization : ICommand
    {
        public string TenantId { get; }
        public string Domain { get; }
        public string OrganizationName { get; }
        public UpdateTenantSubscriptionOrganization(string tenantId, string organizationName, string domain)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new System.ArgumentException($"'{nameof(tenantId)}' cannot be null or whitespace", nameof(tenantId));
            }

            if (string.IsNullOrWhiteSpace(organizationName))
            {
                throw new System.ArgumentException($"'{nameof(organizationName)}' cannot be null or whitespace", nameof(organizationName));
            }

            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new System.ArgumentException($"'{nameof(domain)}' cannot be null or whitespace", nameof(domain));
            }

            this.TenantId = tenantId;
            this.OrganizationName = organizationName;
            this.Domain = domain;
        }
    }
}
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Tenants.Events
{
    [MessageNamespace("tenants")]
    public class TenantOrganizationUpdated : IEvent
    {
        public TenantOrganizationUpdated(string organizationName, string domain, bool domainWasUpdated)
        {
            if (string.IsNullOrWhiteSpace(organizationName))
            {
                throw new System.ArgumentException($"'{nameof(organizationName)}' cannot be null or whitespace", nameof(organizationName));
            }

            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new System.ArgumentException($"'{nameof(domain)}' cannot be null or whitespace", nameof(domain));
            }

            this.OrganizationName = organizationName;
            this.Domain = domain;
            this.DomainWasUpdated = domainWasUpdated;
        }
        public string OrganizationName { get; }
        public string Domain { get; }
        public bool DomainWasUpdated { get; }

    }
}
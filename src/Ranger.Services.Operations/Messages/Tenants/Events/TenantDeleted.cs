using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Tenants.Events
{
    [MessageNamespace("tenants")]
    public class TenantDeleted : IEvent
    {
        public TenantDeleted(string domainName, string organizationName)
        {
            if (string.IsNullOrWhiteSpace(domainName))
            {
                throw new System.ArgumentException($"{nameof(domainName)} was null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(organizationName))
            {
                throw new System.ArgumentException($"{nameof(organizationName)} was null or whitespace.");
            }

            this.DomainName = domainName;
            this.OrganizationName = organizationName;
        }

        public string DomainName { get; }
        public string OrganizationName { get; }
    }
}
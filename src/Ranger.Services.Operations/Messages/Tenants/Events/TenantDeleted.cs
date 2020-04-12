using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Tenants.Events
{
    [MessageNamespace("tenants")]
    public class TenantDeleted : IEvent
    {
        public TenantDeleted(string tenantId, string organizationName)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new System.ArgumentException($"{nameof(tenantId)} was null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(organizationName))
            {
                throw new System.ArgumentException($"{nameof(organizationName)} was null or whitespace.");
            }

            this.TenantId = tenantId;
            this.OrganizationName = organizationName;
        }

        public string TenantId { get; }
        public string OrganizationName { get; }
    }
}
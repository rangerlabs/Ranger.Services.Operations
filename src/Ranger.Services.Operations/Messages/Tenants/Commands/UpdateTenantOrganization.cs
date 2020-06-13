using System;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations
{
    [MessageNamespace("tenants")]
    public class UpdateTenantOrganization : ICommand
    {
        public UpdateTenantOrganization(string commandingUserEmail, string tenantid, int version, string organizationName, string domain)
        {
            if (string.IsNullOrWhiteSpace(commandingUserEmail))
            {
                throw new ArgumentException($"'{nameof(commandingUserEmail)}' cannot be null or whitespace", nameof(commandingUserEmail));
            }

            if (string.IsNullOrWhiteSpace(tenantid))
            {
                throw new ArgumentException($"'{nameof(tenantid)}' cannot be null or whitespace", nameof(tenantid));
            }

            if (string.IsNullOrWhiteSpace(organizationName) && string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"'{nameof(organizationName)}' or '{nameof(domain)}' must not null or whitespace", nameof(organizationName));
            }

            if (version <= 1)
            {
                throw new ArgumentException($"'{nameof(version)}' must be greater than 1", nameof(version));
            }

            CommandingUserEmail = commandingUserEmail;
            TenantId = tenantid;
            OrganizationName = organizationName;
            Domain = domain;
            Version = version;
        }

        public string Domain { get; }
        public string OrganizationName { get; }
        public string TenantId { get; }
        public string CommandingUserEmail { get; }
        public int Version { get; }
    }
}
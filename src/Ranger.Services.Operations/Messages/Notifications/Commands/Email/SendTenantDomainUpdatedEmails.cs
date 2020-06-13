using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Notifications.Commands.Email
{
    [MessageNamespace("notifications")]
    public class SendTenantDomainUpdatedEmails : ICommand
    {
        public SendTenantDomainUpdatedEmails(string tenantId, string domain, string commandingUserEmail)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new System.ArgumentException($"'{nameof(tenantId)}' cannot be null or whitespace", nameof(tenantId));
            }

            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new System.ArgumentException($"'{nameof(domain)}' cannot be null or whitespace", nameof(domain));
            }

            if (string.IsNullOrWhiteSpace(commandingUserEmail))
            {
                throw new System.ArgumentException($"'{nameof(commandingUserEmail)}' cannot be null or whitespace", nameof(commandingUserEmail));
            }

            this.TenantId = tenantId;
            this.Domain = domain;
            this.CommandingUserEmail = commandingUserEmail;
        }
        public string TenantId { get; }
        public string Domain { get; }
        public string CommandingUserEmail { get; }
    }
}
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Notifications
{
    [MessageNamespace("notifications")]
    public class SendDomainDeletedEmail : ICommand
    {
        public string Email { get; }
        public string FirstName { get; }
        public string TenantId { get; }

        public SendDomainDeletedEmail(string email, string firstName, string tenantId)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new System.ArgumentException(nameof(email));
            }
            if (string.IsNullOrWhiteSpace(firstName))
            {
                throw new System.ArgumentNullException(nameof(firstName));
            }
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new System.ArgumentNullException(nameof(tenantId));
            }

            this.Email = email;
            this.FirstName = firstName;
            this.TenantId = tenantId;
        }
    }
}
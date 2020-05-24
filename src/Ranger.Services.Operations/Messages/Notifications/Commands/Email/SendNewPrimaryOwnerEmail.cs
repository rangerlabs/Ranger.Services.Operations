using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Notifications
{
    [MessageNamespace("notifications")]
    public class SendNewPrimaryOwnerEmail : ICommand
    {
        public string Email { get; }
        public string FirstName { get; }
        public string TenantId { get; }
        public string Token { get; }

        public SendNewPrimaryOwnerEmail(string email, string firstName, string tenantId, string token)
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

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new System.ArgumentNullException(nameof(token));
            }

            this.Email = email;
            this.FirstName = firstName;
            this.TenantId = tenantId;
            this.Token = token;
        }
    }
}
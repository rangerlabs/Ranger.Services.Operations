using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Notifications
{
    [MessageNamespace("notifications")]
    public class SendNewTenantOwnerEmail : ICommand
    {
        public string Email { get; }
        public string FirstName { get; }
        public string Domain { get; }
        public string Token { get; }

        public SendNewTenantOwnerEmail(string email, string firstName, string domain, string token)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new System.ArgumentException(nameof(email));
            }

            if (string.IsNullOrWhiteSpace(firstName))
            {
                throw new System.ArgumentNullException(nameof(firstName));
            }

            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new System.ArgumentNullException(nameof(domain));
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new System.ArgumentNullException(nameof(token));
            }

            this.Email = email;
            this.FirstName = firstName;
            this.Domain = domain;
            this.Token = token;
        }
    }
}
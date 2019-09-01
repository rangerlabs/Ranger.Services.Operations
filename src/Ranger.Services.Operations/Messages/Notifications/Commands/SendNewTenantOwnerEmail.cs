using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Notifications
{
    [MessageNamespace("notifications")]
    public class SendNewTenantOwnerEmail : ICommand
    {
        public string Email { get; }
        public string FirstName { get; }
        public string Domain { get; }
        public string RegistrationCode { get; }

        public SendNewTenantOwnerEmail(string email, string firstName, string domain, string registrationCode)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new System.ArgumentException(nameof(email));
            }

            if (firstName is null)
            {
                throw new System.ArgumentNullException(nameof(firstName));
            }

            if (domain is null)
            {
                throw new System.ArgumentNullException(nameof(domain));
            }

            if (registrationCode is null)
            {
                throw new System.ArgumentNullException(nameof(registrationCode));
            }

            this.Email = email;
            this.FirstName = firstName;
            this.Domain = domain;
            this.RegistrationCode = registrationCode;
        }
    }
}
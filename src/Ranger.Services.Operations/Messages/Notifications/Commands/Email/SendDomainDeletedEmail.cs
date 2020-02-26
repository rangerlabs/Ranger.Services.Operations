using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Notifications
{
    [MessageNamespace("notifications")]
    public class SendDomainDeletedEmail : ICommand
    {
        public string Email { get; }
        public string FirstName { get; }
        public string OrganizationName { get; }

        public SendDomainDeletedEmail(string email, string firstName, string organizationName)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new System.ArgumentException(nameof(email));
            }
            if (string.IsNullOrWhiteSpace(firstName))
            {
                throw new System.ArgumentNullException(nameof(firstName));
            }
            if (string.IsNullOrWhiteSpace(organizationName))
            {
                throw new System.ArgumentNullException(nameof(organizationName));
            }

            this.Email = email;
            this.FirstName = firstName;
            this.OrganizationName = organizationName;
        }
    }
}
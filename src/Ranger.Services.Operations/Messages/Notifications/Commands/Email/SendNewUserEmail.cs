using System.Collections.Generic;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Notifications
{
    [MessageNamespace("notifications")]
    public class SendNewUserEmail : ICommand
    {
        public string Email { get; }
        public string FirstName { get; }
        public string Domain { get; }
        public string Role { get; }
        public string RegistrationCode { get; }
        public IEnumerable<string> AuthorizedProjects { get; }

        public SendNewUserEmail(string email, string firstName, string domain, string role, string registrationCode, IEnumerable<string> authorizedProjects = null)
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

            if (string.IsNullOrWhiteSpace(role))
            {
                throw new System.ArgumentNullException(nameof(role));
            }

            if (string.IsNullOrWhiteSpace(registrationCode))
            {
                throw new System.ArgumentNullException(nameof(registrationCode));
            }

            this.Email = email;
            this.FirstName = firstName;
            this.Domain = domain;
            this.Role = role;
            this.RegistrationCode = registrationCode;
            this.AuthorizedProjects = authorizedProjects;

        }
    }
}
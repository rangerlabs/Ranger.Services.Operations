using System.Collections.Generic;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Notifications
{
    [MessageNamespace("notifications")]
    public class SendUserRoleUpdatedEmail : ICommand
    {
        public string UserId { get; }
        public string Email { get; }
        public string FirstName { get; }
        public string Domain { get; }
        public string Organization { get; }
        public string Role { get; }
        public IEnumerable<string> AuthorizedProjects { get; }

        public SendUserRoleUpdatedEmail(string userId, string email, string firstName, string domain, string organization, string role, IEnumerable<string> authorizedProjects = null)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new System.ArgumentException(nameof(userId));
            }

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

            if (string.IsNullOrWhiteSpace(organization))
            {
                throw new System.ArgumentNullException(nameof(organization));
            }

            if (string.IsNullOrWhiteSpace(role))
            {
                throw new System.ArgumentNullException(nameof(role));
            }

            this.UserId = userId;
            this.Email = email;
            this.FirstName = firstName;
            this.Domain = domain;
            this.Organization = organization;
            this.Role = role;
            this.AuthorizedProjects = authorizedProjects;

        }
    }
}
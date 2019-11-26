using System.Collections.Generic;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Notifications
{
    [MessageNamespace("notifications")]
    public class SendUserPermissionsUpdatedEmail : ICommand
    {
        public string UserId { get; }
        public string Email { get; }
        public string FirstName { get; }
        public string Domain { get; }
        public string Role { get; }
        public IEnumerable<string> AuthorizedProjects { get; }

        public SendUserPermissionsUpdatedEmail(string userId, string email, string firstName, string domain, string role, IEnumerable<string> authorizedProjects = null)
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

            if (string.IsNullOrWhiteSpace(role))
            {
                throw new System.ArgumentNullException(nameof(role));
            }

            this.UserId = userId;
            this.Email = email;
            this.FirstName = firstName;
            this.Domain = domain;
            this.Role = role;
            this.AuthorizedProjects = authorizedProjects;

        }
    }
}
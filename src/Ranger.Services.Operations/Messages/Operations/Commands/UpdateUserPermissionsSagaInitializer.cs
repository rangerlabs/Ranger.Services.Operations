using System.Collections.Generic;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Operations
{
    [MessageNamespace("operations")]
    public class UpdateUserPermissionsSagaInitializer : SagaInitializer, ICommand
    {
        public string Email { get; }
        public string CommandingUserEmail { get; }
        public string Role { get; }
        public IEnumerable<string> AuthorizedProjects { get; }

        public UpdateUserPermissionsSagaInitializer(string domain, string email, string commandingUserEmail, string role = "", IEnumerable<string> authorizedProjects = null)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new System.ArgumentException($"{nameof(domain)} was null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new System.ArgumentException($"{nameof(email)} was null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(commandingUserEmail))
            {
                throw new System.ArgumentException($"{nameof(commandingUserEmail)} was null or whitespace.");
            }

            this.Email = email;
            this.CommandingUserEmail = commandingUserEmail;
            Domain = domain;
            this.Role = role;
            this.AuthorizedProjects = authorizedProjects;
        }
    }
}
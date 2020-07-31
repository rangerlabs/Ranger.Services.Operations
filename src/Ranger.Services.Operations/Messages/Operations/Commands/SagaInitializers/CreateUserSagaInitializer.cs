using System;
using System.Collections.Generic;
using Ranger.Common;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Operations
{
    [MessageNamespaceAttribute("operations")]
    public class CreateUserSagaInitializer : SagaInitializer, ICommand
    {
        public string Email { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public RolesEnum Role { get; }
        public string CommandingUserEmail { get; }
        public IEnumerable<Guid> AuthorizedProjects { get; }

        public CreateUserSagaInitializer(string tenantId, string email, string firstName, string lastName, RolesEnum role, string commandingUserEmail, IEnumerable<Guid> authorizedProjects)
        {
            if (string.IsNullOrEmpty(tenantId))
            {
                throw new System.ArgumentException($"{nameof(tenantId)} was null or whitespace");
            }

            if (string.IsNullOrEmpty(email))
            {
                throw new System.ArgumentException($"{nameof(email)} was null or whitespace");
            }

            if (string.IsNullOrEmpty(firstName))
            {
                throw new System.ArgumentException($"{nameof(firstName)} was null or whitespace");
            }

            if (string.IsNullOrEmpty(lastName))
            {
                throw new System.ArgumentException($"{nameof(lastName)} was null or whitespace");
            }

            if (string.IsNullOrEmpty(commandingUserEmail))
            {
                throw new System.ArgumentException($"{nameof(commandingUserEmail)} was null or whitespace");
            }

            TenantId = tenantId;
            this.Email = email;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Role = role;
            this.CommandingUserEmail = commandingUserEmail;
            this.AuthorizedProjects = authorizedProjects ?? new List<Guid>();
        }
    }
}
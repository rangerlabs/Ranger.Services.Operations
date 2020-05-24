using System;
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
        public IEnumerable<Guid> AuthorizedProjects { get; }

        public UpdateUserPermissionsSagaInitializer(string tenantId, string email, string commandingUserEmail, string role = "", IEnumerable<Guid> authorizedProjects = null)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new System.ArgumentException($"{nameof(tenantId)} was null or whitespace");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new System.ArgumentException($"{nameof(email)} was null or whitespace");
            }

            if (string.IsNullOrWhiteSpace(commandingUserEmail))
            {
                throw new System.ArgumentException($"{nameof(commandingUserEmail)} was null or whitespace");
            }

            this.Email = email;
            this.CommandingUserEmail = commandingUserEmail;
            TenantId = tenantId;
            this.Role = role;
            this.AuthorizedProjects = authorizedProjects;
        }
    }
}
using System;
using System.Collections.Generic;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Notifications
{
    [MessageNamespace("notifications")]
    public class SendNewUserEmail : ICommand
    {
        public string UserId { get; }
        public string Email { get; }
        public string FirstName { get; }
        public string TenantId { get; }
        public string Role { get; }
        public string Token { get; }
        public IEnumerable<string> AuthorizedProjects { get; }

        public SendNewUserEmail(string userId, string email, string firstName, string tenantId, string role, string token, IEnumerable<string> authorizedProjects = null)
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

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new System.ArgumentNullException(nameof(tenantId));
            }

            if (string.IsNullOrWhiteSpace(role))
            {
                throw new System.ArgumentNullException(nameof(role));
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new System.ArgumentNullException(nameof(token));
            }

            this.UserId = userId;
            this.Email = email;
            this.FirstName = firstName;
            this.TenantId = tenantId;
            this.Role = role;
            this.Token = token;
            this.AuthorizedProjects = authorizedProjects;

        }
    }
}
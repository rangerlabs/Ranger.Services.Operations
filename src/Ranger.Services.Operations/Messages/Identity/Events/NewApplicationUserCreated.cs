using System.Collections.Generic;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations
{
    [MessageNamespace("identity")]
    public class NewApplicationUserCreated : IEvent
    {
        public string Domain { get; }
        public string Email { get; }
        public string FirstName { get; }
        public string Role { get; }
        public string RegistrationKey { get; }
        public string CommandingUserEmail { get; }
        public IEnumerable<string> PermittedProjects { get; }

        public NewApplicationUserCreated(string domain, string email, string firstName, string role, string registrationKey, string commandingUserEmail, IEnumerable<string> permittedProjects = null)
        {

            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new System.ArgumentException($"{nameof(domain)} was null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new System.ArgumentException($"{nameof(email)} was null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(firstName))
            {
                throw new System.ArgumentException($"{nameof(firstName)} was null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(role))
            {
                throw new System.ArgumentException($"{nameof(role)} was null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(registrationKey))
            {
                throw new System.ArgumentException($"{nameof(registrationKey)} was null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(commandingUserEmail))
            {
                throw new System.ArgumentException($"{nameof(commandingUserEmail)} was null or whitespace.");
            }


            this.Domain = domain;
            this.Email = email;
            this.FirstName = firstName;
            this.Role = role;
            this.RegistrationKey = registrationKey;
            this.CommandingUserEmail = commandingUserEmail;
            this.PermittedProjects = permittedProjects;
        }
    }
}
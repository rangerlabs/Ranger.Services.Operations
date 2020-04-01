using System;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Subscriptions
{
    [MessageNamespace("subscriptions")]
    public class CreateNewTenantSubscription : ICommand
    {
        public CreateNewTenantSubscription(string domain, string commandingUserEmail, string firstName, string lastName, string organizationName, string pgsqlDatabaseUsername)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new System.ArgumentException($"{nameof(domain)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(commandingUserEmail))
            {
                throw new System.ArgumentException($"{nameof(commandingUserEmail)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(firstName))
            {
                throw new System.ArgumentException($"{nameof(firstName)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(lastName))
            {
                throw new System.ArgumentException($"{nameof(lastName)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(organizationName))
            {
                throw new ArgumentException($"{nameof(organizationName)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(pgsqlDatabaseUsername))
            {
                throw new ArgumentException($"{nameof(pgsqlDatabaseUsername)} was null or whitespace.");
            }

            this.Domain = domain;
            this.CommandingUserEmail = commandingUserEmail;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.OrganizationName = organizationName;
            this.PgsqlDatabaseUsername = pgsqlDatabaseUsername;

        }
        public string Domain { get; }
        public string CommandingUserEmail { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string OrganizationName { get; }
        public string PgsqlDatabaseUsername { get; }
    }
}
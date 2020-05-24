using System;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Subscriptions
{
    [MessageNamespace("subscriptions")]
    public class CreateNewTenantSubscription : ICommand
    {
        public CreateNewTenantSubscription(string tenantId, string commandingUserEmail, string firstName, string lastName, string organizationName)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new System.ArgumentException($"{nameof(tenantId)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(commandingUserEmail))
            {
                throw new System.ArgumentException($"{nameof(commandingUserEmail)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(firstName))
            {
                throw new System.ArgumentException($"{nameof(firstName)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(lastName))
            {
                throw new System.ArgumentException($"{nameof(lastName)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(organizationName))
            {
                throw new ArgumentException($"{nameof(organizationName)} was null or whitespace");
            }

            this.TenantId = tenantId;
            this.CommandingUserEmail = commandingUserEmail;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.OrganizationName = organizationName;

        }
        public string TenantId { get; }
        public string CommandingUserEmail { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string OrganizationName { get; }
    }
}
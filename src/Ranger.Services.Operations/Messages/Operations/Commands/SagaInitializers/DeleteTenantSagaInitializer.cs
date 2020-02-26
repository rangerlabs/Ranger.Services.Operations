using System;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Operations
{
    [MessageNamespaceAttribute("operations")]
    public class DeleteTenantSagaInitializer : SagaInitializer, ICommand
    {
        public DeleteTenantSagaInitializer(string commandingUserEmail, string domain)
        {
            if (string.IsNullOrWhiteSpace(commandingUserEmail))
            {
                throw new System.ArgumentException($"{nameof(commandingUserEmail)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new System.ArgumentException($"{nameof(domain)} was null or whitespace.");
            }

            this.CommandingUserEmail = commandingUserEmail;
            Domain = domain;
        }

        public string CommandingUserEmail { get; }
    }
}
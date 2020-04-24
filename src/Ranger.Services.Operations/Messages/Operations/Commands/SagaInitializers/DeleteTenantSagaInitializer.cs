using System;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Operations
{
    [MessageNamespaceAttribute("operations")]
    public class DeleteTenantSagaInitializer : SagaInitializer, ICommand
    {
        public DeleteTenantSagaInitializer(string commandingUserEmail, string tenantId)
        {
            if (string.IsNullOrWhiteSpace(commandingUserEmail))
            {
                throw new System.ArgumentException($"{nameof(commandingUserEmail)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new System.ArgumentException($"{nameof(tenantId)} was null or whitespace");
            }

            this.CommandingUserEmail = commandingUserEmail;
            TenantId = tenantId;
        }

        public string CommandingUserEmail { get; }
    }
}
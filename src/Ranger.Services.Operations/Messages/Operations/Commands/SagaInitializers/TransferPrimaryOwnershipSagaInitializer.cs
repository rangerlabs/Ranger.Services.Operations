using Ranger.RabbitMQ;
using Ranger.Services.Operations.Messages.Operations;

namespace Ranger.Services.Operations
{
    [MessageNamespace("operations")]
    public class TransferPrimaryOwnershipSagaInitializer : SagaInitializer, ICommand
    {
        public TransferPrimaryOwnershipSagaInitializer(string commandingUserEmail,
            string transferUserEmail,
            string tenantId)
        {
            if (string.IsNullOrWhiteSpace(commandingUserEmail))
            {
                throw new System.ArgumentException($"{nameof(commandingUserEmail)} was null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(transferUserEmail))
            {
                throw new System.ArgumentException($"{nameof(transferUserEmail)} was null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new System.ArgumentException($"{nameof(tenantId)} was null or whitespace.");
            }

            CommandingUserEmail = commandingUserEmail;
            TransferUserEmail = transferUserEmail;
            TenantId = tenantId;
        }

        public string CommandingUserEmail { get; private set; }
        public string TransferUserEmail { get; private set; }
    }
}
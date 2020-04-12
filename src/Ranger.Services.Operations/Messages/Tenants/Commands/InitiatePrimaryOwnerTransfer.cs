using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Tenants.Commands
{
    [MessageNamespace("tenants")]
    public class InitiatePrimaryOwnerTransfer : ICommand
    {
        public InitiatePrimaryOwnerTransfer(string tenantId, string commandingUserEmail, string transferUserEmail)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new System.ArgumentException($"{nameof(tenantId)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(commandingUserEmail))
            {
                throw new System.ArgumentException($"{nameof(commandingUserEmail)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(transferUserEmail))
            {
                throw new System.ArgumentException($"{nameof(transferUserEmail)} was null or whitespace.");
            }

            this.TenantId = tenantId;
            this.CommandingUserEmail = commandingUserEmail;
            this.TransferUserEmail = transferUserEmail;
        }

        public string TenantId { get; }
        public string CommandingUserEmail { get; }
        public string TransferUserEmail { get; }
    }
}
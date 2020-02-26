using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Tenants.Commands
{
    [MessageNamespace("tenants")]
    public class InitiatePrimaryOwnerTransfer : ICommand
    {
        public InitiatePrimaryOwnerTransfer(string domain, string commandingUserEmail, string transferUserEmail)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new System.ArgumentException($"{nameof(domain)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(commandingUserEmail))
            {
                throw new System.ArgumentException($"{nameof(commandingUserEmail)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(transferUserEmail))
            {
                throw new System.ArgumentException($"{nameof(transferUserEmail)} was null or whitespace.");
            }

            this.Domain = domain;
            this.CommandingUserEmail = commandingUserEmail;
            this.TransferUserEmail = transferUserEmail;
        }

        public string Domain { get; }
        public string CommandingUserEmail { get; }
        public string TransferUserEmail { get; }
    }
}
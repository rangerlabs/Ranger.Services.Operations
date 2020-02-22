using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Operations.Commands
{
    [MessageNamespace("operations")]
    public class CancelPrimaryOwnershipTransfer
    {
        public string Domain { get; }
        public string CommandingUserEmail { get; }

        public CancelPrimaryOwnershipTransfer(string domain, string commandingUserEmail)
        {
            this.Domain = domain;
            this.CommandingUserEmail = commandingUserEmail;
        }
    }
}
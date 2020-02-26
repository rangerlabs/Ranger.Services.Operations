using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Operations.Commands
{
    [MessageNamespace("operations")]
    public class CancelPrimaryOwnershipTransfer : ICommand
    {
        public CancelPrimaryOwnershipTransfer()
        {
        }
    }
}
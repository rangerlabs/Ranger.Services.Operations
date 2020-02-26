using Ranger.RabbitMQ;

namespace Ranger.Services.Operations
{
    [MessageNamespace("operations")]
    public class RefusePrimaryOwnershipTransfer : ICommand
    {
        public RefusePrimaryOwnershipTransfer()
        { }
    }
}
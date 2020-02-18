using Ranger.RabbitMQ;

namespace Ranger.Services.Operations
{
    [MessageNamespace("operations")]
    public class PrimaryOwnershipTransferRefused : IEvent
    {
        public PrimaryOwnershipTransferRefused()
        { }
    }
}
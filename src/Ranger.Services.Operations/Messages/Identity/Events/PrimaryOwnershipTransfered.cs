using Ranger.RabbitMQ;

namespace Ranger.Services.Operations
{
    [MessageNamespace("identity")]
    public class PrimaryOwnershipTransfered : IEvent
    { }
}
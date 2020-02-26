using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Tenants.Events
{
    [MessageNamespace("tenants")]
    public class PrimaryOwnerTransferCompleted : IEvent
    {
    }
}
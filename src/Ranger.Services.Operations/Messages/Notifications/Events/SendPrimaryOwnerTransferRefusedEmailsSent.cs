using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Notifications
{
    [MessageNamespaceAttribute("notifications")]
    public class SendPrimaryOwnerTransferRefusedEmailsSent : IEvent
    {
        public SendPrimaryOwnerTransferRefusedEmailsSent() { }
    }
}
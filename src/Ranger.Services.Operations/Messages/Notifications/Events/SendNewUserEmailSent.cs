using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Notifications
{
    [MessageNamespaceAttribute("notifications")]
    public class SendNewUserEmailSent : IEvent
    {
        public SendNewUserEmailSent() { }
    }
}
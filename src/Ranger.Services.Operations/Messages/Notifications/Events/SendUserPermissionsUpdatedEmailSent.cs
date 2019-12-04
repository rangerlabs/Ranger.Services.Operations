using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Notifications
{
    [MessageNamespaceAttribute("notifications")]
    public class SendUserPermissionsUpdatedEmailSent : IEvent
    {
        public SendUserPermissionsUpdatedEmailSent() { }
    }
}
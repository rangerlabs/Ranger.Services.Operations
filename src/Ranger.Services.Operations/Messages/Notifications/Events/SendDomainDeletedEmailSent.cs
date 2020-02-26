using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Notifications.Events
{
    [MessageNamespaceAttribute("notifications")]
    public class SendDomainDeletedEmailSent : IEvent
    { }
}
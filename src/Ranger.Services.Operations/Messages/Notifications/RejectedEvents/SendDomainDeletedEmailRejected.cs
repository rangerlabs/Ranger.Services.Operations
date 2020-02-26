using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Notifications
{
    [MessageNamespaceAttribute("notifications")]
    public class SendDomainDeletedEmailRejected : IRejectedEvent
    {
        public string Reason { get; }
        public string Code { get; }

        public SendDomainDeletedEmailRejected(string message, string code)
        {
            this.Reason = message;
            this.Code = code;
        }
    }
}
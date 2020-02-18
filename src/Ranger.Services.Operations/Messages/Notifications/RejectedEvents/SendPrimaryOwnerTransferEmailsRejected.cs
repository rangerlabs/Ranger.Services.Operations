using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Notifications
{
    [MessageNamespaceAttribute("notifications")]
    public class SendPrimaryOwnerTransferEmailsRejected : IRejectedEvent
    {
        public string Reason { get; }
        public string Code { get; }

        public SendPrimaryOwnerTransferEmailsRejected(string message, string code)
        {
            this.Reason = message;
            this.Code = code;
        }
    }
}
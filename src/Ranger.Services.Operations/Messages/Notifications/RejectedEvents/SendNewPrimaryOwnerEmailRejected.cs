using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Notifications
{
    [MessageNamespaceAttribute("notifications")]
    public class SendNewPrimaryOwnerEmailRejected : IRejectedEvent
    {
        public string Reason { get; }
        public string Code { get; }

        public SendNewPrimaryOwnerEmailRejected(string message, string code)
        {
            this.Reason = message;
            this.Code = code;
        }
    }
}
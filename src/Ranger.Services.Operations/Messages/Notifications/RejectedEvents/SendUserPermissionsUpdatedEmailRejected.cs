using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Notifications
{
    [MessageNamespaceAttribute("notifications")]
    public class SendUserPermissionsUpdatedEmailRejected : IRejectedEvent
    {
        public string Reason { get; }
        public string Code { get; }

        public SendUserPermissionsUpdatedEmailRejected(string message, string code)
        {
            this.Reason = message;
            this.Code = code;
        }
    }
}
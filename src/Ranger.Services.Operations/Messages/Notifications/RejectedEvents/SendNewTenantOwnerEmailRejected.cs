using Ranger.RabbitMQ;

namespace Ranger.Services.Operations {
    [MessageNamespaceAttribute ("notifications")]
    public class SendNewTenantOwnerEmailRejected : IRejectedEvent {
        public string Reason { get; }
        public string Code { get; }

        public SendNewTenantOwnerEmailRejected (string message, string code) {
            this.Reason = message;
            this.Code = code;
        }
    }
}
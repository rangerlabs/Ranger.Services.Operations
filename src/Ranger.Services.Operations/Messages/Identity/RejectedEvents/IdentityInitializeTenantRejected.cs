using Ranger.RabbitMQ;

namespace Ranger.Services.Operations {
    [MessageNamespaceAttribute ("identity")]
    public class IdentityInitializeTenantRejected : IRejectedEvent {
        public string Reason { get; }
        public string Code { get; }

        public IdentityInitializeTenantRejected (string reason, string code) {
            this.Reason = reason;
            this.Code = code;
        }
    }
}
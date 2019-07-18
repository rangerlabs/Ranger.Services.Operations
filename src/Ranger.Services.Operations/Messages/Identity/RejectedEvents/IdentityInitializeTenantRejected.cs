using Ranger.RabbitMQ;

namespace Ranger.Services.Operations {
    [MessageNamespaceAttribute ("identity")]
    public class IdentityInitializeTenantRejected : IRejectedEvent {
        public CorrelationContext CorrelationContext { get; }
        public string Reason { get; }
        public string Code { get; }

        public IdentityInitializeTenantRejected (CorrelationContext correlationContext, string reason, string code) {
            this.CorrelationContext = correlationContext;
            this.Reason = reason;
            this.Code = code;
        }
    }
}
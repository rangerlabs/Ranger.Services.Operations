using Ranger.RabbitMQ;

namespace Ranger.Services.Operations {
    [MessageNamespaceAttribute ("identity")]
    public class IdentityTenantInitialized : IEvent {
        public CorrelationContext CorrelationContext { get; }

        public IdentityTenantInitialized (CorrelationContext correlationContext) {
            this.CorrelationContext = correlationContext;
        }
    }
}
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations {
    [MessageNamespace ("tenants")]
    public class TenantCreated : IEvent {
        public CorrelationContext CorrelationContext { get; set; }
        public string Domain { get; set; }
    }
}
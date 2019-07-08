using Ranger.RabbitMQ;

namespace Ranger.Services.Operations {
    [MessageNamespace ("tenants")]
    public class TenantCreated : IEvent {
        public TenantCreated (CorrelationContext correlationContext, string domainName, User user) {
            this.CorrelationContext = correlationContext;
            this.DomainName = domainName;
            this.User = user;

        }
        public CorrelationContext CorrelationContext { get; }
        public string DomainName { get; }
        public User User { get; }
    }
}
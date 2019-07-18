using Ranger.RabbitMQ;

namespace Ranger.Services.Operations {
    [MessageNamespace ("tenants")]
    public class TenantCreated : IEvent {
        public TenantCreated (CorrelationContext correlationContext, string domainName, string databaseUsername, string databasePassword, User user) {
            this.CorrelationContext = correlationContext;
            this.DomainName = domainName;
            this.DatabaseUsername = databaseUsername;
            this.DatabasePassword = databasePassword;
            this.User = user;

        }
        public CorrelationContext CorrelationContext { get; }
        public string DomainName { get; }
        public string DatabaseUsername { get; }
        public string DatabasePassword { get; }
        public User User { get; }
    }
}
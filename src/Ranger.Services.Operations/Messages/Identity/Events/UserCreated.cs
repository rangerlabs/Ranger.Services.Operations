using Ranger.RabbitMQ;

namespace Ranger.Services.Operations {
    [MessageNamespaceAttribute ("identity")]
    public class UserCreated : IEvent {
        public UserCreated (string email, string firstName, string lastName, string tenantDomain, string role, CorrelationContext correlationContext) {
            this.Email = email;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.TenantDomain = tenantDomain;
            this.Role = role;
            this.CorrelationContext = correlationContext;

        }
        public string Email { get; }

        public string FirstName { get; }

        public string LastName { get; }

        public string TenantDomain { get; }

        public string Role { get; }
        public CorrelationContext CorrelationContext { get; set; }

    }
}
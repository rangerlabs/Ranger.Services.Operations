using Ranger.RabbitMQ;

namespace Ranger.Services.Operations {
    [MessageNamespaceAttribute ("identity")]
    public class CreateUser : ICommand {
        public string Email { get; }

        public string FirstName { get; }

        public string LastName { get; }

        public string PasswordHash { get; }

        public string TenantDomain { get; }

        public string Role { get; }
        public string PhoneNumber { get; }
        public CorrelationContext CorrelationContext { get; set; }

        public CreateUser (string email, string firstName, string lastName, string passwordHash, string tenantDomain, string role, string phoneNumber, CorrelationContext correlationContext) {
            this.Email = email;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.PasswordHash = passwordHash;
            this.TenantDomain = tenantDomain;
            this.Role = role;
            this.PhoneNumber = phoneNumber;
            this.CorrelationContext = correlationContext;
        }
    }
}
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations {
    [MessageNamespaceAttribute ("identity")]
    public class CreateUser : ICommand {
        public CreateUser (string email, string firstName, string lastName, string passwordHash, string domainName, string role, string phoneNumber, CorrelationContext correlationContext) {
            this.Email = email;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.PasswordHash = passwordHash;
            this.DomainName = domainName;
            this.Role = role;
            this.PhoneNumber = phoneNumber;
            this.CorrelationContext = correlationContext;
        }
        public string Email { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string PasswordHash { get; }
        public string DomainName { get; }
        public string Role { get; }
        public string PhoneNumber { get; }
        public CorrelationContext CorrelationContext { get; }

    }
}
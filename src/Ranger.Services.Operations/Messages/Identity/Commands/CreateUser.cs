using Ranger.RabbitMQ;

namespace Ranger.Services.Operations {
    [MessageNamespaceAttribute ("identity")]
    public class CreateUser : ICommand {
        public CreateUser (string email, string firstName, string lastName, string password, string domainName, string role, string phoneNumber) {
            this.Email = email;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Password = password;
            this.DomainName = domainName;
            this.Role = role;
            this.PhoneNumber = phoneNumber;
        }
        public string Email { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string Password { get; }
        public string DomainName { get; }
        public string Role { get; }
        public string PhoneNumber { get; }

    }
}
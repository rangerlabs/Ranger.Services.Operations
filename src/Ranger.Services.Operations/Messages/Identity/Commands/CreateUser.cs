using Ranger.RabbitMQ;

namespace Ranger.Services.Operations
{
    [MessageNamespaceAttribute("identity")]
    public class CreateUser : ICommand
    {
        public CreateUser(string email, string firstName, string lastName, string role, string domain)
        {
            this.Email = email;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Role = role;
            this.Domain = domain;
        }
        public string Email { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string Role { get; }
        public string Domain { get; }
    }
}
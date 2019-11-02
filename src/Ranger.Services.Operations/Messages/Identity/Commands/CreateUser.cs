using Ranger.RabbitMQ;

namespace Ranger.Services.Operations
{
    [MessageNamespaceAttribute("identity")]
    public class CreateUser : ICommand
    {
        public CreateUser(string email, string firstName, string lastName, string role, string domain, string commandingUserEmail)
        {
            this.Email = email;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Role = role;
            this.Domain = domain;
            this.CommandingUserEmail = commandingUserEmail;
        }
        public string Email { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string Role { get; }
        public string Domain { get; }
        public string CommandingUserEmail { get; }
    }
}
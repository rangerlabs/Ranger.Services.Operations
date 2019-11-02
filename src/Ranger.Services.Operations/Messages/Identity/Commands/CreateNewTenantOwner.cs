using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Identity
{
    [MessageNamespaceAttribute("identity")]
    public class CreateNewTenantOwner : ICommand
    {
        public CreateNewTenantOwner(string email, string firstName, string lastName, string password, string domain, string comandingUserEmail)
        {
            this.Email = email;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Password = password;
            this.Domain = domain;
            this.CommandingUserEmail = CommandingUserEmail;
        }
        public string Email { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string Password { get; }
        public string Domain { get; }
        public string CommandingUserEmail { get; }
    }
}
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Tenants
{

    [MessageNamespace("tenants")]
    public class CreateTenant : ICommand
    {

        public CreateTenant(string domainName, string organizationName, string email, string firstName, string lastName, string password)
        {
            if (string.IsNullOrWhiteSpace(domainName))
            {
                throw new System.ArgumentException($"{nameof(domainName)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(organizationName))
            {
                throw new System.ArgumentException($"{nameof(organizationName)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new System.ArgumentException($"{nameof(email)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(firstName))
            {
                throw new System.ArgumentException($"{nameof(firstName)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(lastName))
            {
                throw new System.ArgumentException($"{nameof(lastName)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new System.ArgumentException($"{nameof(password)} was null or whitespace.");
            }
            this.DomainName = domainName;
            this.OrganizationName = organizationName;
            this.Email = email;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Password = password;

        }
        public string DomainName { get; }
        public string OrganizationName { get; }
        public string Email { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string Password { get; }

    }
}
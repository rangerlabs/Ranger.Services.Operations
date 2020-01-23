using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Tenants
{
    [MessageNamespace("tenants")]
    public class TenantCreated : IEvent
    {
        public TenantCreated(string domain, string email, string firstName, string lastName, string password, string organizationName, string databaseUsername, string databasePassword, string token)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new System.ArgumentException($"{nameof(domain)} was null or whitespace.");
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
            if (string.IsNullOrWhiteSpace(databaseUsername))
            {
                throw new System.ArgumentException($"{nameof(databaseUsername)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(databasePassword))
            {
                throw new System.ArgumentException($"{nameof(databasePassword)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new System.ArgumentException($"{nameof(token)} was null or whitespace.");
            }
            this.Domain = domain;
            this.Email = email;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Password = password;
            this.OrganizationName = organizationName;
            this.DatabaseUsername = databaseUsername;
            this.DatabasePassword = databasePassword;
            this.Token = token;
        }
        public string Domain { get; }
        public string Email { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string Password { get; }
        public string OrganizationName { get; }
        public string DatabaseUsername { get; }
        public string DatabasePassword { get; }
        public string Token { get; }
    }
}
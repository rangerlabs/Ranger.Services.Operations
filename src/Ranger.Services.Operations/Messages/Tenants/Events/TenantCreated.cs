using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Tenants
{
    [MessageNamespace("tenants")]
    public class TenantCreated : IEvent
    {
        public TenantCreated(string tenantId, string email, string firstName, string lastName, string password, string organizationName, string databasePassword, string token)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new System.ArgumentException($"{nameof(tenantId)} was null or whitespace.");
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
            if (string.IsNullOrWhiteSpace(databasePassword))
            {
                throw new System.ArgumentException($"{nameof(databasePassword)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new System.ArgumentException($"{nameof(token)} was null or whitespace.");
            }
            this.TenantId = tenantId;
            this.Email = email;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Password = password;
            this.OrganizationName = organizationName;
            this.DatabasePassword = databasePassword;
            this.Token = token;
        }
        public string TenantId { get; }
        public string Email { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string Password { get; }
        public string OrganizationName { get; }
        public string DatabasePassword { get; }
        public string Token { get; }
    }
}
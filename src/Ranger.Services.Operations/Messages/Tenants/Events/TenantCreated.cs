using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Tenants
{
    [MessageNamespace("tenants")]
    public class TenantCreated : IEvent
    {
        public TenantCreated(string domainName, string databaseUsername, string databasePassword, string registrationKey, NewTenantOwner owner)
        {
            this.DomainName = domainName;
            this.DatabaseUsername = databaseUsername;
            this.DatabasePassword = databasePassword;
            this.RegistrationKey = registrationKey;
            this.Owner = owner;

        }
        public string DomainName { get; }
        public string DatabaseUsername { get; }
        public string DatabasePassword { get; }
        public string RegistrationKey { get; }
        public NewTenantOwner Owner { get; }
    }
}
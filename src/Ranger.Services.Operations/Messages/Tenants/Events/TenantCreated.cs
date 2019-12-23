using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Tenants
{
    [MessageNamespace("tenants")]
    public class TenantCreated : IEvent
    {
        public TenantCreated(string domainName, string databaseUsername, string databasePassword, string token, NewPrimaryOwner owner)
        {
            this.DomainName = domainName;
            this.DatabaseUsername = databaseUsername;
            this.DatabasePassword = databasePassword;
            this.Token = token;
            this.Owner = owner;

        }
        public string DomainName { get; }
        public string DatabaseUsername { get; }
        public string DatabasePassword { get; }
        public string Token { get; }
        public NewPrimaryOwner Owner { get; }
    }
}
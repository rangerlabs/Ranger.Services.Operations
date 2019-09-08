using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Tenants
{
    [MessageNamespace("tenants")]
    public class DeleteTenant : ICommand
    {
        public DeleteTenant(string domain)
        {
            this.Domain = domain;
        }
        public string Domain { get; }
    }
}
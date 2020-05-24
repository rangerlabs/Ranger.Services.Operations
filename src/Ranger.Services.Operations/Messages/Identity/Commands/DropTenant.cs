using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Identity
{
    [MessageNamespace("identity")]
    public class DropTenant : ICommand
    {
        public string TenantId { get; }

        public DropTenant(string tenantId)
        {
            this.TenantId = tenantId;
        }
    }
}
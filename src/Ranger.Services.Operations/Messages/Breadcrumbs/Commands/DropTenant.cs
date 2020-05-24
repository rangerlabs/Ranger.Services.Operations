using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Breadcrumbs
{
    [MessageNamespace("breadcrumbs")]
    public class DropTenant : ICommand
    {
        public string TenantId { get; }

        public DropTenant(string tenantId)
        {
            this.TenantId = tenantId;
        }
    }
}
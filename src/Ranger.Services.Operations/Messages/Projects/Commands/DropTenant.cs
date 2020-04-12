using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Projects
{
    [MessageNamespace("projects")]
    public class DropTenant : ICommand
    {
        public string TenantId { get; }

        public DropTenant(string tenantId)
        {
            this.TenantId = tenantId;
        }
    }
}
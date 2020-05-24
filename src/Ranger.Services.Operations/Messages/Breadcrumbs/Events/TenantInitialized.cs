using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Breadcrumbs
{
    [MessageNamespaceAttribute("breadcrumbs")]
    public class TenantInitialized : IEvent
    {
        public TenantInitialized() { }
    }
}
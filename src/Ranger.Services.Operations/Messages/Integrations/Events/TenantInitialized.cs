using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Integrations
{
    [MessageNamespaceAttribute("integrations")]
    public class TenantInitialized : IEvent
    {
        public TenantInitialized() { }
    }
}
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Identity
{
    [MessageNamespaceAttribute("identity")]
    public class TenantInitialized : IEvent
    {
        public TenantInitialized() { }
    }
}
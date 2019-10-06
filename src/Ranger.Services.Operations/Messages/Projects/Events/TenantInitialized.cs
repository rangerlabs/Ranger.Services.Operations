using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Projects
{
    [MessageNamespaceAttribute("projects")]
    public class TenantInitialized : IEvent
    {
        public TenantInitialized() { }
    }
}
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Geofences
{
    [MessageNamespaceAttribute("geofences")]
    public class TenantInitialized : IEvent
    {
        public TenantInitialized() { }
    }
}
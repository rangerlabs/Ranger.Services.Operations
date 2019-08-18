using Ranger.RabbitMQ;

namespace Ranger.Services.Operations {
    [MessageNamespaceAttribute ("geofences")]
    public class GeofencesTenantInitialized : IEvent {

        public GeofencesTenantInitialized () { }
    }
}
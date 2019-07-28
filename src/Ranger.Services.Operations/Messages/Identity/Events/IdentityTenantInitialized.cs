using Ranger.RabbitMQ;

namespace Ranger.Services.Operations {
    [MessageNamespaceAttribute ("identity")]
    public class IdentityTenantInitialized : IEvent {

        public IdentityTenantInitialized () { }
    }
}
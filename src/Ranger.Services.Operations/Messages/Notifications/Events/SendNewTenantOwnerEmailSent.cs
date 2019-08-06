using Ranger.RabbitMQ;

namespace Ranger.Services.Operations {
    [MessageNamespaceAttribute ("notifications")]
    public class SendNewTenantOwnerEmailSent : IEvent {
        public SendNewTenantOwnerEmailSent () { }
    }
}
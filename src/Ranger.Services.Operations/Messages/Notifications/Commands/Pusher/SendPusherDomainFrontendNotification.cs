
using Ranger.RabbitMQ;
using Ranger.Services.Operations.Data;

namespace Ranger.Services.Operations
{
    [MessageNamespace("notifications")]
    public class SendPusherDomainFrontendNotification : ICommand
    {
        public string BackendEventKey { get; }
        public string TenantId { get; }
        public OperationsStateEnum State { get; }

        public SendPusherDomainFrontendNotification(string backendEventKey, string tenantId, OperationsStateEnum state)
        {
            this.BackendEventKey = backendEventKey;
            this.TenantId = tenantId;
            this.State = state;
        }
    }
}
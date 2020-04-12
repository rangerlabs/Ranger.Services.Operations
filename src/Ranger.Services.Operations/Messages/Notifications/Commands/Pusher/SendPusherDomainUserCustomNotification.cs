using System;
using Ranger.RabbitMQ;
using Ranger.Services.Operations.Data;

namespace Ranger.Services.Operations
{
    [MessageNamespace("notifications")]
    public class SendPusherDomainUserCustomNotification : ICommand
    {
        public string EventName { get; }
        public string Message { get; }
        public string TenantId { get; }
        public string UserEmail { get; }
        public OperationsStateEnum State { get; }
        public Guid ResourceId { get; }

        public SendPusherDomainUserCustomNotification(string eventName, string message, string tenantId, string userEmail, OperationsStateEnum state, Guid resourceId = default(Guid))
        {
            this.EventName = eventName;
            this.Message = message;
            this.TenantId = tenantId;
            this.UserEmail = userEmail;
            this.State = state;
            this.ResourceId = resourceId;
        }
    }
}
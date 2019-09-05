using Ranger.RabbitMQ;
using Ranger.Services.Operations.Data;

namespace Ranger.Services.Operations
{
    [MessageNamespace("notifications")]
    public class SendPusherPrivateFrontendNotification : ICommand
    {
        public string BackendEventKey { get; }
        public string Domain { get; }
        public string UserEmail { get; }
        public OperationsStateEnum State { get; }

        public SendPusherPrivateFrontendNotification(string backendEventName, string domain, string userEmail, OperationsStateEnum state)
        {
            this.BackendEventKey = backendEventName;
            this.Domain = domain;
            this.UserEmail = userEmail;
            this.State = state;
        }
    }
}
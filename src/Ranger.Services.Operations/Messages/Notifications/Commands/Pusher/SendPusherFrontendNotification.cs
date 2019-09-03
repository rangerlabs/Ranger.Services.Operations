using System;
using Ranger.RabbitMQ;
using Ranger.Services.Operations.Data;

namespace Ranger.Services.Operations
{
    [MessageNamespace("notifications")]
    public class SendPusherFrontendNotification : IEvent
    {
        public string BackendEventName { get; }
        public string Domain { get; }
        public string UserEmail { get; }
        public OperationsStateEnum State { get; }

        public SendPusherFrontendNotification(string backendEventName, string domain, string userEmail, OperationsStateEnum state)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException(nameof(domain));
            }

            if (string.IsNullOrWhiteSpace(userEmail))
            {
                throw new ArgumentException(nameof(userEmail));
            }

            this.BackendEventName = backendEventName;
            this.Domain = domain;
            this.UserEmail = userEmail;
            this.State = state;
        }
    }
}
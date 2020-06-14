using System;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations
{
    [MessageNamespace("notifications")]
    public class SendPusherOrganizationDomainUpdatedNotification : ICommand
    {
        public string EventName { get; }
        public string Message { get; }
        public string OldDomain { get; }

        public SendPusherOrganizationDomainUpdatedNotification(string eventName, string message, string oldDomain)
        {
            this.EventName = eventName;
            this.Message = message;
            this.OldDomain = oldDomain;
        }
    }
}
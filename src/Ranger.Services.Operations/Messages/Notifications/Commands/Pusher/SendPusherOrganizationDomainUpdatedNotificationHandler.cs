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
        public string NewDomain { get; }

        public SendPusherOrganizationDomainUpdatedNotification(string eventName, string message, string oldDomain, string newDomain)
        {
            if (string.IsNullOrWhiteSpace(eventName))
            {
                throw new ArgumentException($"'{nameof(eventName)}' cannot be null or whitespace", nameof(eventName));
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException($"'{nameof(message)}' cannot be null or whitespace", nameof(message));
            }

            if (string.IsNullOrWhiteSpace(oldDomain))
            {
                throw new ArgumentException($"'{nameof(oldDomain)}' cannot be null or whitespace", nameof(oldDomain));
            }

            if (string.IsNullOrWhiteSpace(newDomain))
            {
                throw new ArgumentException($"'{nameof(newDomain)}' cannot be null or whitespace", nameof(newDomain));
            }

            this.EventName = eventName;
            this.Message = message;
            this.OldDomain = oldDomain;
            this.NewDomain = newDomain;
        }
    }
}
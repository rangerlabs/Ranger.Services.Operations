using System;
using Ranger.Common;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Subscriptions
{
    [MessageNamespace("subscriptions")]
    public class ResourceCountIncremented : IEvent
    {
        public string Domain { get; }
        public ResourceEnum Resource { get; }
        public int NewCount { get; }

        public ResourceCountIncremented(string domain, ResourceEnum resource, int newCount)
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} was null or whitespace.");
            }
            if (newCount < 1)
            {
                throw new ArgumentException($"{nameof(newCount)} was less than 1.");
            }
            this.Domain = domain;
            this.Resource = resource;
            this.NewCount = newCount;
        }
    }
}
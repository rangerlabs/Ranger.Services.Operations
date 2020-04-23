using System;
using Ranger.Common;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Subscriptions
{
    [MessageNamespace("subscriptions")]
    public class ResourceCountIncremented : IEvent
    {
        public ResourceEnum Resource { get; }
        public int NewCount { get; }

        public ResourceCountIncremented(ResourceEnum resource, int newCount)
        {
            if (newCount < 1)
            {
                throw new ArgumentException($"{nameof(newCount)} was less than 1.");
            }
            this.Resource = resource;
            this.NewCount = newCount;
        }
    }
}
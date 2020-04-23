using System;
using Ranger.Common;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Subscriptions
{
    [MessageNamespace("subscriptions")]
    public class ResourceCountDecremented : IEvent
    {
        public ResourceEnum Resource { get; }
        public int NewCount { get; }

        public ResourceCountDecremented(ResourceEnum resource, int newCount)
        {
            if (newCount < 0)
            {
                throw new ArgumentException($"{nameof(newCount)} was less than 0.");
            }
            this.Resource = resource;
            this.NewCount = newCount;
        }
    }
}
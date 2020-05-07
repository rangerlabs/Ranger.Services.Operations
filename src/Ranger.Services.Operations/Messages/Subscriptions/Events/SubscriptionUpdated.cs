using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Subscriptions.Messages.Events
{
    [MessageNamespace("subscriptions")]
    public class SubscriptionUpdated : IEvent
    {
        public SubscriptionUpdated()
        { }
    }
}
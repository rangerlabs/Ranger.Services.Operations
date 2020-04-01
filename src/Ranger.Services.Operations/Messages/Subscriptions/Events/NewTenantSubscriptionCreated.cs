
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Subscriptions
{
    [MessageNamespace("subscriptions")]
    public class NewTenantSubscriptionCreated : IEvent
    { }
}
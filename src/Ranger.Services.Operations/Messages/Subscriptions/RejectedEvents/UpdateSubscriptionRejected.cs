using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Subscriptions.RejectedEvents
{
    [MessageNamespace("subscriptions")]
    public class UpdateSubscriptionRejected : IRejectedEvent
    {
        public UpdateSubscriptionRejected(string reason, string code)
        {
            this.Reason = reason;
            this.Code = code;
        }
        public string Reason { get; }
        public string Code { get; }
    }
}
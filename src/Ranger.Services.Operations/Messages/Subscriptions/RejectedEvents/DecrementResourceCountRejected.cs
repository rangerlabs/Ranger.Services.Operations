using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Subscriptions
{
    [MessageNamespace("subscriptions")]
    public class DecrementResourceCountRejected : IRejectedEvent
    {
        public DecrementResourceCountRejected(string reason, string code)
        {
            this.Reason = reason;
            this.Code = code;
        }

        public string Reason { get; }
        public string Code { get; }
    }
}
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Subscriptions.RejectedEvents
{
    public class CancelTenantSubscriptionRejected : IRejectedEvent
    {
        public CancelTenantSubscriptionRejected(string reason, string code)
        {
            this.Reason = reason;
            this.Code = code;

        }
        public string Reason { get; }
        public string Code { get; }
    }
}
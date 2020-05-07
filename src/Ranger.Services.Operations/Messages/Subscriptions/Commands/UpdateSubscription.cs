using System;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Subscriptions.Commands
{
    [MessageNamespace("subscriptions")]
    public class UpdateSubscription : ICommand
    {
        public UpdateSubscription(string tenantId, string subscriptionId, string planId, bool active, DateTime? scheduledCancellationDate)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new System.ArgumentException($"{nameof(tenantId)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(subscriptionId))
            {
                throw new System.ArgumentException($"{nameof(subscriptionId)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(planId))
            {
                throw new System.ArgumentException($"{nameof(planId)} was null or whitespace");
            }

            this.TenantId = tenantId;
            this.SubscriptionId = subscriptionId;
            this.PlanId = planId;
            this.Active = active;
            this.ScheduledCancellationDate = scheduledCancellationDate;

        }
        public string TenantId { get; }
        public string SubscriptionId { get; }
        public string PlanId { get; }
        public bool Active { get; }
        public DateTime? ScheduledCancellationDate { get; }
    }
}
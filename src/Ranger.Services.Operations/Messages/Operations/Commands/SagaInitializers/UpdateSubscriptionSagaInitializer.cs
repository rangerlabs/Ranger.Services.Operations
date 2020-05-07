using System;
using ChargeBee.Models.Enums;
using Ranger.RabbitMQ;
using Ranger.Services.Operations.Messages.Operations;

namespace Ranger.Services.Operations
{
    [MessageNamespace("operations")]
    public class UpdateSubscriptionSagaInitializer : SagaInitializer
    {
        public UpdateSubscriptionSagaInitializer(string tenantId, string subscriptionId, string planId, EventTypeEnum chargeBeeSubscriptionEvent, bool active = true, DateTime? scheduledCancellationDate = null)
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
            if (
                !(chargeBeeSubscriptionEvent is EventTypeEnum.SubscriptionChanged) &&
                !(chargeBeeSubscriptionEvent is EventTypeEnum.SubscriptionPaused) &&
                !(chargeBeeSubscriptionEvent is EventTypeEnum.SubscriptionResumed) &&
                !(chargeBeeSubscriptionEvent is EventTypeEnum.SubscriptionCancelled) &&
                !(chargeBeeSubscriptionEvent is EventTypeEnum.SubscriptionReactivated) &&
                !(chargeBeeSubscriptionEvent is EventTypeEnum.SubscriptionCancellationReminder) &&
                !(chargeBeeSubscriptionEvent is EventTypeEnum.SubscriptionScheduledCancellationRemoved)
            )
            {
                throw new System.ArgumentException($"{nameof(chargeBeeSubscriptionEvent)} was an incorrect type. Must be SubscriptionChanged, *Paushed, or *Resumed");
            }

            TenantId = tenantId;
            SubscriptionId = subscriptionId;
            PlanId = planId;
            ChargeBeeSubscriptionEvent = chargeBeeSubscriptionEvent;
            Active = active;
            ScheduledCancellationDate = scheduledCancellationDate;
        }

        public string SubscriptionId { get; }
        public string PlanId { get; }
        public bool Active { get; }
        public EventTypeEnum ChargeBeeSubscriptionEvent { get; }
        public DateTime? ScheduledCancellationDate { get; }
    }
}
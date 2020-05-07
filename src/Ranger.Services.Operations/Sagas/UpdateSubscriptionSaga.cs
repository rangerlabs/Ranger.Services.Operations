using System;
using System.Threading.Tasks;
using ChargeBee.Models.Enums;
using Chronicle;
using Microsoft.Extensions.Logging;
using Ranger.RabbitMQ;
using Ranger.Services.Operations.Data;
using Ranger.Services.Operations.Messages.Subscriptions.Commands;
using Ranger.Services.Operations.Messages.Subscriptions.Messages.Events;
using Ranger.Services.Operations.Messages.Subscriptions.RejectedEvents;

namespace Ranger.Services.Operations.Sagas
{
    public class UpdateSubscriptionSaga : Saga<UpdateSubscriptionSagaData>,
        ISagaStartAction<UpdateSubscriptionSagaInitializer>,
        ISagaAction<SubscriptionUpdated>,
        ISagaAction<UpdateSubscriptionRejected>
    {
        private readonly IBusPublisher busPublisher;
        private readonly ILogger<UpdateSubscriptionSaga> logger;

        public UpdateSubscriptionSaga(IBusPublisher busPublisher, ILogger<UpdateSubscriptionSaga> logger)
        {
            this.busPublisher = busPublisher;
            this.logger = logger;
        }

        public Task CompensateAsync(UpdateSubscriptionSagaInitializer message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(SubscriptionUpdated message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(UpdateSubscriptionRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task HandleAsync(UpdateSubscriptionSagaInitializer message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            Data.TenantId = message.TenantId;
            Data.ChargeBeeSubscriptionEvent = message.ChargeBeeSubscriptionEvent;
            busPublisher.Send(new UpdateSubscription(message.TenantId, message.SubscriptionId, message.PlanId, message.Active, message.ScheduledCancellationDate), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public async Task HandleAsync(SubscriptionUpdated message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            busPublisher.Send(new SendPusherDomainCustomNotification("subscription-changed", "Your subscription has changed. Retrieving new subscription details", Data.TenantId), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            await CompleteAsync();
        }

        public async Task HandleAsync(UpdateSubscriptionRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            busPublisher.Send(new SendPusherDomainCustomNotification("subscription-change", message.Reason, Data.TenantId), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            await RejectAsync();
        }
    }

    public class UpdateSubscriptionSagaData : BaseSagaData
    {
        public EventTypeEnum ChargeBeeSubscriptionEvent { get; set; }
    }
}
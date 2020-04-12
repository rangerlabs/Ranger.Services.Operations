using System;
using System.Threading.Tasks;
using Chronicle;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.RabbitMQ;
using Ranger.Services.Operations.Data;
using Ranger.Services.Operations.Messages.Geofences;
using Ranger.Services.Operations.Messages.Operations;
using Ranger.Services.Operations.Messages.Subscriptions;

namespace Ranger.Services.Operations.Sagas
{
    public class DeleteGeofenceSaga : Saga<DeleteGeofenceData>,
        ISagaStartAction<DeleteGeofenceSagaInitializer>,
        ISagaAction<GeofenceDeleted>,
        ISagaAction<ResourceCountDecremented>,
        ISagaAction<DeleteGeofenceRejected>,
        ISagaAction<DecrementResourceCountRejected>
    {
        private readonly ILogger<DeleteGeofenceSaga> logger;
        private readonly IBusPublisher busPublisher;

        public DeleteGeofenceSaga(IBusPublisher busPublisher, ILogger<DeleteGeofenceSaga> logger)
        {
            this.busPublisher = busPublisher;
            this.logger = logger;
        }

        public Task CompensateAsync(DeleteGeofenceSagaInitializer message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(GeofenceDeleted message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(DeleteGeofenceRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(ResourceCountDecremented message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(DecrementResourceCountRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            return Task.CompletedTask;
        }

        public Task HandleAsync(DeleteGeofenceSagaInitializer message, ISagaContext context)
        {

            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            Data.TenantId = message.TenantId;
            Data.Message = message;
            Data.Initiator = message.CommandingUserEmailOrTokenPrefix;
            busPublisher.Send(new DecrementResourceCount(Data.TenantId, ResourceEnum.Geofence), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public async Task HandleAsync(GeofenceDeleted message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            if (Data.Message.FrontendRequest)
            {
                busPublisher.Send(new SendPusherDomainUserCustomNotification("geofence-deleted", $"Geofence '{Data.Message.ExternalId}' was successfully deleted.", Data.TenantId, Data.Initiator, OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                await CompleteAsync();
            }
            else
            {
                await CompleteAsync();
            }
        }

        public async Task HandleAsync(DeleteGeofenceRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            if (Data.Message.FrontendRequest)
            {
                if (!string.IsNullOrWhiteSpace(message.Reason))
                {
                    busPublisher.Send(new SendPusherDomainUserCustomNotification("geofence-deleted", $"An error occurred deleting geofence '{Data.Message.ExternalId}'. {message.Reason}", Data.TenantId, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                }
                else
                {
                    busPublisher.Send(new SendPusherDomainUserCustomNotification("geofence-deleted", $"An error occurred deleting geofence '{Data.Message.ExternalId}'.", Data.TenantId, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                }
                await RejectAsync();
            }
            else
            {
                await RejectAsync();
            }
        }

        public Task HandleAsync(ResourceCountDecremented message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            var deleteGeofence = new DeleteGeofence(
                Data.Message.CommandingUserEmailOrTokenPrefix,
                Data.Message.TenantId,
                Data.Message.ExternalId,
                Data.Message.ProjectId
            );
            busPublisher.Send(deleteGeofence, CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public Task HandleAsync(DecrementResourceCountRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            logger.LogCritical($"Failed to decrement the Geofence utilization for tenant domain '{Data.TenantId}'! Reason: {message.Reason}.");
            return RejectAsync();
        }
    }

    public class DeleteGeofenceData : BaseSagaData
    {
        public DeleteGeofenceSagaInitializer Message;
    }
}
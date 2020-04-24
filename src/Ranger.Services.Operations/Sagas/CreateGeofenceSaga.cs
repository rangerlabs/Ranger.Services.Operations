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

namespace Ranger.Services.Operations
{
    public class CreateGeofenceSaga : Saga<CreateGeofenceData>,
        ISagaStartAction<CreateGeofenceSagaInitializer>,
        ISagaAction<GeofenceCreated>,
        ISagaAction<ResourceCountIncremented>,
        ISagaAction<CreateGeofenceRejected>,
        ISagaAction<IncrementResourceCountRejected>
    {
        private readonly ILogger<CreateGeofenceSaga> logger;
        private readonly IBusPublisher busPublisher;

        public CreateGeofenceSaga(IBusPublisher busPublisher, ILogger<CreateGeofenceSaga> logger)
        {
            this.busPublisher = busPublisher;
            this.logger = logger;
        }

        public Task CompensateAsync(CreateGeofenceSagaInitializer message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(GeofenceCreated message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            busPublisher.Send(new DeleteGeofence("System", Data.TenantId, Data.Message.ExternalId, Data.Message.ProjectId), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public Task CompensateAsync(CreateGeofenceRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(ResourceCountIncremented message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(IncrementResourceCountRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task HandleAsync(CreateGeofenceSagaInitializer message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            Data.Message = message;
            Data.TenantId = message.TenantId;
            Data.Initiator = message.CommandingUserEmailOrTokenPrefix;
            busPublisher.Send(new IncrementResourceCount(Data.TenantId, ResourceEnum.Geofence), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public async Task HandleAsync(GeofenceCreated message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            Data.Id = message.Id;
            if (Data.Message.FrontendRequest)
            {
                busPublisher.Send(new SendPusherDomainUserCustomNotification("geofence-created", $"Geofence '{Data.Message.ExternalId}' was successfully created", Data.TenantId, Data.Initiator, OperationsStateEnum.Completed, Data.Id), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                await CompleteAsync();
            }
            else
            {
                await CompleteAsync();
            }
        }

        public async Task HandleAsync(CreateGeofenceRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            if (Data.Message.FrontendRequest)
            {
                if (!string.IsNullOrWhiteSpace(message.Reason))
                {
                    busPublisher.Send(new SendPusherDomainUserCustomNotification("geofence-created", $"An error occurred creating geofence '{Data.Message.ExternalId}'. {message.Reason}", Data.TenantId, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                }
                else
                {
                    busPublisher.Send(new SendPusherDomainUserCustomNotification("geofence-created", $"An error occurred creating geofence '{Data.Message.ExternalId}'", Data.TenantId, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                }
                await RejectAsync();
            }
            else
            {
                await RejectAsync();
            }
        }

        public Task HandleAsync(ResourceCountIncremented message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            var createGeofence = new CreateGeofence(
                Data.Message.CommandingUserEmailOrTokenPrefix,
                Data.Message.TenantId,
                Data.Message.ExternalId,
                Data.Message.ProjectId,
                Data.Message.Shape,
                Data.Message.Coordinates,
                Data.Message.Labels,
                Data.Message.IntegrationIds,
                Data.Message.Metadata,
                Data.Message.Description,
                Data.Message.Radius,
                Data.Message.Enabled,
                Data.Message.OnEnter,
                Data.Message.OnDwell,
                Data.Message.OnExit,
                Data.Message.ExpirationDate,
                Data.Message.LaunchDate,
                Data.Message.Schedule
            );
            busPublisher.Send(createGeofence, CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public async Task HandleAsync(IncrementResourceCountRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            if (Data.Message.FrontendRequest)
            {
                busPublisher.Send(new SendPusherDomainUserCustomNotification("geofence-created", $"Failed to create geofence '{Data.Message.ExternalId}'. Subscription limit reached", Data.TenantId, Data.Initiator, OperationsStateEnum.Rejected, Data.Id), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            }
            await RejectAsync();
        }
    }

    public class CreateGeofenceData : BaseSagaData
    {
        public CreateGeofenceSagaInitializer Message;
        public Guid Id;
    }
}
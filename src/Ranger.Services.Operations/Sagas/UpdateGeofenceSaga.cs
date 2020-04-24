using System;
using System.Threading.Tasks;
using Chronicle;
using Microsoft.Extensions.Logging;
using Ranger.RabbitMQ;
using Ranger.Services.Operations.Data;
using Ranger.Services.Operations.Messages.Geofences;
using Ranger.Services.Operations.Messages.Operations;

namespace Ranger.Services.Operations.Sagas
{
    public class UpsertGeofenceSaga : Saga<UpsertGeofenceData>,
        ISagaStartAction<UpdateGeofenceSagaInitializer>,
        ISagaAction<GeofenceUpdated>,
        ISagaAction<UpdateGeofenceRejected>
    {
        private readonly ILogger<UpsertGeofenceSaga> logger;
        private readonly IBusPublisher busPublisher;

        public UpsertGeofenceSaga(IBusPublisher busPublisher, ILogger<UpsertGeofenceSaga> logger)
        {
            this.busPublisher = busPublisher;
            this.logger = logger;
        }

        public Task CompensateAsync(UpdateGeofenceSagaInitializer message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(GeofenceUpdated message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(UpdateGeofenceRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task HandleAsync(UpdateGeofenceSagaInitializer message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            Data.FrontendRequest = message.FrontendRequest;
            Data.TenantId = message.TenantId;
            Data.Initiator = message.CommandingUserEmailOrTokenPrefix;
            Data.ExternalId = message.ExternalId;

            var updateGeofence = new UpdateGeofence(
                message.CommandingUserEmailOrTokenPrefix,
                message.TenantId,
                message.Id,
                message.ExternalId,
                message.ProjectId,
                message.Shape,
                message.Coordinates,
                message.Labels,
                message.IntegrationIds,
                message.Metadata,
                message.Description,
                message.Radius,
                message.Enabled,
                message.OnEnter,
                message.OnDwell,
                message.OnExit,
                message.ExpirationDate,
                message.LaunchDate,
                message.Schedule
            );
            busPublisher.Send(updateGeofence, CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public async Task HandleAsync(GeofenceUpdated message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            if (Data.FrontendRequest)
            {
                busPublisher.Send(new SendPusherDomainUserCustomNotification("geofence-updated", $"Geofence '{Data.ExternalId}' was successfully updated", Data.TenantId, Data.Initiator, OperationsStateEnum.Completed, message.Id), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                await CompleteAsync();
            }
            else
            {
                await CompleteAsync();
            }
        }

        public async Task HandleAsync(UpdateGeofenceRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            if (Data.FrontendRequest)
            {
                if (!string.IsNullOrWhiteSpace(message.Reason))
                {
                    busPublisher.Send(new SendPusherDomainUserCustomNotification("geofence-updated", $"An error occurred updating geofence '{Data.ExternalId}'. {message.Reason}", Data.TenantId, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                }
                else
                {
                    busPublisher.Send(new SendPusherDomainUserCustomNotification("geofence-updated", $"An error occurred updating geofence '{Data.ExternalId}'", Data.TenantId, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                }
                await RejectAsync();
            }
            else
            {
                await RejectAsync();
            }
        }
    }

    public class UpsertGeofenceData : BaseSagaData
    {
        public bool FrontendRequest;
        public string ExternalId;
    }
}
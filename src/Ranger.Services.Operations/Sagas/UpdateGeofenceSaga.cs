using System;
using System.Threading.Tasks;
using Chronicle;
using Microsoft.Extensions.Logging;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;
using Ranger.Services.Operations.Data;
using Ranger.Services.Operations.Messages.Geofences;
using Ranger.Services.Operations.Messages.Operations;

namespace Ranger.Services.Operations.Sagas
{
    public class UpsertGeofenceSaga : BaseSaga<UpsertGeofenceSaga, UpsertGeofenceData>,
        ISagaStartAction<UpdateGeofenceSagaInitializer>,
        ISagaAction<GeofenceUpdated>,
        ISagaAction<UpdateGeofenceRejected>
    {
        private readonly ILogger<UpsertGeofenceSaga> logger;
        private readonly IBusPublisher busPublisher;
        private readonly ITenantsClient tenantsClient;

        public UpsertGeofenceSaga(IBusPublisher busPublisher, ITenantsClient tenantsClient, ILogger<UpsertGeofenceSaga> logger) : base(tenantsClient, logger)
        {
            this.tenantsClient = tenantsClient;
            this.busPublisher = busPublisher;
            this.logger = logger;
        }

        public async Task CompensateAsync(UpdateGeofenceSagaInitializer message, ISagaContext context)
        {
            logger.LogInformation("Calling compensate for UpsertGeofenceSagaInitializer.");
            await Task.CompletedTask;
        }

        public async Task CompensateAsync(GeofenceUpdated message, ISagaContext context)
        {
            logger.LogInformation("Calling compensate for GeofenceUpserted.");
            await Task.CompletedTask;
        }

        public async Task CompensateAsync(UpdateGeofenceRejected message, ISagaContext context)
        {
            logger.LogInformation("Calling compensate for UpsertGeofenceRejected.");
            await Task.CompletedTask;
        }

        public async Task HandleAsync(UpdateGeofenceSagaInitializer message, ISagaContext context)
        {
            var databaseUsername = await GetPgsqlDatabaseUsernameOrReject(message);
            Data.DatabaseUsername = databaseUsername;
            Data.FrontendRequest = message.FrontendRequest;
            Data.Domain = message.Domain;
            Data.Initiator = message.CommandingUserEmailOrTokenPrefix;
            Data.ExternalId = message.ExternalId;

            var updateGeofence = new UpdateGeofence(
                message.CommandingUserEmailOrTokenPrefix,
                message.Domain,
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
                message.OnExit,
                message.ExpirationDate,
                message.LaunchDate,
                message.Schedule
            );
            busPublisher.Send(updateGeofence, CorrelationContext.FromId(Guid.Parse(context.SagaId)));
        }

        public async Task HandleAsync(GeofenceUpdated message, ISagaContext context)
        {
            logger.LogInformation("Calling handle for GeofenceUpdated.");
            if (Data.FrontendRequest)
            {
                busPublisher.Send(new SendPusherDomainUserCustomNotification("geofence-updated", $"Geofence '{Data.ExternalId}' was successfully updated.", Data.Domain, Data.Initiator, OperationsStateEnum.Completed, message.Id), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                await CompleteAsync();
            }
            else
            {
                await CompleteAsync();
            }
        }

        public async Task HandleAsync(UpdateGeofenceRejected message, ISagaContext context)
        {
            logger.LogInformation("Calling handle for UpdateGeofenceRejected.");
            if (Data.FrontendRequest)
            {
                if (!string.IsNullOrWhiteSpace(message.Reason))
                {
                    busPublisher.Send(new SendPusherDomainUserCustomNotification("geofence-updated", $"An error occurred updating geofence '{Data.ExternalId}'. {message.Reason}", Data.Domain, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                }
                else
                {
                    busPublisher.Send(new SendPusherDomainUserCustomNotification("geofence-updated", $"An error occurred updating geofence '{Data.ExternalId}'.", Data.Domain, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
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
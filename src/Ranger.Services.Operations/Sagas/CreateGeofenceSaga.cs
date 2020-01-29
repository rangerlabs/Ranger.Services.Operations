using System;
using System.Threading.Tasks;
using Chronicle;
using Microsoft.Extensions.Logging;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;
using Ranger.Services.Operations.Data;
using Ranger.Services.Operations.Messages.Geofences;
using Ranger.Services.Operations.Messages.Operations;

namespace Ranger.Services.Operations
{
    public class CreateGeofenceSaga : BaseSaga<CreateGeofenceSaga, CreateGeofenceData>,
        ISagaStartAction<CreateGeofenceSagaInitializer>,
        ISagaAction<GeofenceCreated>,
        ISagaAction<CreateGeofenceRejected>
    {
        private readonly ILogger<CreateGeofenceSaga> logger;
        private readonly IBusPublisher busPublisher;
        private readonly ITenantsClient tenantsClient;

        public CreateGeofenceSaga(IBusPublisher busPublisher, ITenantsClient tenantsClient, ILogger<CreateGeofenceSaga> logger) : base(tenantsClient, logger)
        {
            this.tenantsClient = tenantsClient;
            this.busPublisher = busPublisher;
            this.logger = logger;
        }

        public async Task CompensateAsync(CreateGeofenceSagaInitializer message, ISagaContext context)
        {
            logger.LogInformation("Calling compensate for CreateGeofenceSagaInitializer.");
            await Task.CompletedTask;
        }

        public async Task CompensateAsync(GeofenceCreated message, ISagaContext context)
        {
            logger.LogInformation("Calling compensate for GeofenceCreated.");
            await Task.CompletedTask;
        }

        public async Task CompensateAsync(CreateGeofenceRejected message, ISagaContext context)
        {
            logger.LogInformation("Calling compensate for CreateGeofenceRejected.");
            await Task.CompletedTask;
        }

        public async Task HandleAsync(CreateGeofenceSagaInitializer message, ISagaContext context)
        {
            var databaseUsername = await GetPgsqlDatabaseUsernameOrReject(message);
            Data.DatabaseUsername = databaseUsername;
            Data.FrontendRequest = message.FrontendRequest;
            Data.Domain = message.Domain;
            Data.Initiator = message.CommandingUserEmailOrTokenPrefix;
            Data.ExternalId = message.ExternalId;

            var createGeofence = new CreateGeofence(
                message.CommandingUserEmailOrTokenPrefix,
                message.Domain,
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
            busPublisher.Send(createGeofence, CorrelationContext.FromId(Guid.Parse(context.SagaId)));

        }

        public async Task HandleAsync(GeofenceCreated message, ISagaContext context)
        {
            logger.LogInformation("Calling handle for GeofenceCreated.");
            if (Data.FrontendRequest)
            {
                busPublisher.Send(new SendPusherDomainUserCustomNotification("geofence-created", $"Geofence '{Data.ExternalId}' was successfully created.", Data.Domain, Data.Initiator, OperationsStateEnum.Completed, message.Id), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                await CompleteAsync();
            }
            else
            {
                await CompleteAsync();
            }
        }

        public async Task HandleAsync(CreateGeofenceRejected message, ISagaContext context)
        {
            logger.LogInformation("Calling handle for CreateGeofenceRejected.");
            if (Data.FrontendRequest)
            {
                if (!string.IsNullOrWhiteSpace(message.Reason))
                {
                    busPublisher.Send(new SendPusherDomainUserCustomNotification("geofence-created", $"An error occurred creating geofence '{Data.ExternalId}'. {message.Reason}", Data.Domain, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                }
                else
                {
                    busPublisher.Send(new SendPusherDomainUserCustomNotification("geofence-created", $"An error occurred creating geofence '{Data.ExternalId}'.", Data.Domain, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                }
                await RejectAsync();
            }
            else
            {
                await RejectAsync();
            }
        }
    }

    public class CreateGeofenceData : BaseSagaData
    {
        public bool FrontendRequest;
        public string ExternalId;
    }
}
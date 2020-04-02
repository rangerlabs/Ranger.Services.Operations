using System;
using System.Threading.Tasks;
using Chronicle;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;
using Ranger.Services.Operations.Data;
using Ranger.Services.Operations.Messages.Geofences;
using Ranger.Services.Operations.Messages.Operations;
using Ranger.Services.Operations.Messages.Subscriptions;

namespace Ranger.Services.Operations
{
    public class CreateGeofenceSaga : BaseSaga<CreateGeofenceSaga, CreateGeofenceData>,
        ISagaStartAction<CreateGeofenceSagaInitializer>,
        ISagaAction<GeofenceCreated>,
        ISagaAction<ResourceCountIncremented>,
        ISagaAction<CreateGeofenceRejected>,
        ISagaAction<IncrementResourceCountRejected>
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

        public Task CompensateAsync(CreateGeofenceSagaInitializer message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(GeofenceCreated message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            busPublisher.Send(new DeleteGeofence("System", Data.Domain, Data.ExternalId, Data.ProjectId), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public Task CompensateAsync(CreateGeofenceRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(ResourceCountIncremented message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(IncrementResourceCountRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            return Task.CompletedTask;
        }

        public async Task HandleAsync(CreateGeofenceSagaInitializer message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            var databaseUsername = await GetPgsqlDatabaseUsernameOrReject(message);
            Data.DatabaseUsername = databaseUsername;
            Data.FrontendRequest = message.FrontendRequest;
            Data.Domain = message.Domain;
            Data.Initiator = message.CommandingUserEmailOrTokenPrefix;
            Data.ExternalId = message.ExternalId;
            Data.ProjectId = message.ProjectId;

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
                message.OnDwell,
                message.OnExit,
                message.ExpirationDate,
                message.LaunchDate,
                message.Schedule
            );
            busPublisher.Send(createGeofence, CorrelationContext.FromId(Guid.Parse(context.SagaId)));
        }

        public Task HandleAsync(GeofenceCreated message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            Data.Id = message.Id;
            busPublisher.Send(new IncrementResourceCount(Data.Domain, ResourceEnum.Goefence), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public async Task HandleAsync(CreateGeofenceRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
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

        public async Task HandleAsync(ResourceCountIncremented message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            if (Data.FrontendRequest)
            {
                busPublisher.Send(new SendPusherDomainUserCustomNotification("geofence-created", $"Geofence '{Data.ExternalId}' was successfully created.", Data.Domain, Data.Initiator, OperationsStateEnum.Completed, Data.Id), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                await CompleteAsync();
            }
            else
            {
                await CompleteAsync();
            }
        }

        public async Task HandleAsync(IncrementResourceCountRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            await RejectAsync();
        }
    }

    public class CreateGeofenceData : BaseSagaData
    {
        public Guid Id;
        public Guid ProjectId;
        public bool FrontendRequest;
        public string ExternalId;
    }
}
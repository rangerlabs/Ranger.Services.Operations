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

namespace Ranger.Services.Operations.Sagas
{
    public class DeleteGeofenceSaga : BaseSaga<DeleteGeofenceSaga, DeleteGeofenceData>,
        ISagaStartAction<DeleteGeofenceSagaInitializer>,
        ISagaAction<GeofenceDeleted>,
        ISagaAction<ResourceCountDecremented>,
        ISagaAction<DeleteGeofenceRejected>,
        ISagaAction<DecrementResourceCountRejected>
    {
        private readonly ILogger<DeleteGeofenceSaga> logger;
        private readonly IBusPublisher busPublisher;
        private readonly ITenantsClient tenantsClient;

        public DeleteGeofenceSaga(IBusPublisher busPublisher, ITenantsClient tenantsClient, ILogger<DeleteGeofenceSaga> logger) : base(tenantsClient, logger)
        {
            this.tenantsClient = tenantsClient;
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

        public async Task HandleAsync(DeleteGeofenceSagaInitializer message, ISagaContext context)
        {

            var databaseUsername = await GetPgsqlDatabaseUsernameOrReject(message);
            Data.DatabaseUsername = databaseUsername;
            Data.FrontendRequest = message.FrontendRequest;
            Data.Domain = message.Domain;
            Data.Initiator = message.CommandingUserEmailOrTokenPrefix;
            Data.ExternalId = message.ExternalId;

            var deleteGeofence = new DeleteGeofence(
                message.CommandingUserEmailOrTokenPrefix,
                message.Domain,
                message.ExternalId,
                message.ProjectId
            );
            busPublisher.Send(deleteGeofence, CorrelationContext.FromId(Guid.Parse(context.SagaId)));
        }

        public Task HandleAsync(GeofenceDeleted message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            busPublisher.Send(new DecrementResourceCount(Data.Domain, ResourceEnum.Goefence), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public async Task HandleAsync(DeleteGeofenceRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            if (Data.FrontendRequest)
            {
                if (!string.IsNullOrWhiteSpace(message.Reason))
                {
                    busPublisher.Send(new SendPusherDomainUserCustomNotification("geofence-deleted", $"An error occurred deleting geofence '{Data.ExternalId}'. {message.Reason}", Data.Domain, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                }
                else
                {
                    busPublisher.Send(new SendPusherDomainUserCustomNotification("geofence-deleted", $"An error occurred deleting geofence '{Data.ExternalId}'.", Data.Domain, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                }
                await RejectAsync();
            }
            else
            {
                await RejectAsync();
            }
        }

        public async Task HandleAsync(ResourceCountDecremented message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            if (Data.FrontendRequest)
            {
                busPublisher.Send(new SendPusherDomainUserCustomNotification("geofence-deleted", $"Geofence '{Data.ExternalId}' was successfully deleted.", Data.Domain, Data.Initiator, OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                await CompleteAsync();
            }
            else
            {
                await CompleteAsync();
            }
        }

        public Task HandleAsync(DecrementResourceCountRejected message, ISagaContext context)
        {
            logger.LogCritical($"Failed to decrement the Geofence utilization for tenant domain '{Data.Domain}'! Reason: {message.Reason}.");
            return Task.CompletedTask;
        }
    }

    public class DeleteGeofenceData : BaseSagaData
    {
        public bool FrontendRequest;
        public string ExternalId;
    }
}
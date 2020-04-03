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

            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            var databaseUsername = await GetPgsqlDatabaseUsernameOrReject(message);
            Data.DatabaseUsername = databaseUsername;
            Data.Message = message;
            Data.Domain = message.Domain;
            Data.Initiator = message.CommandingUserEmailOrTokenPrefix;
            busPublisher.Send(new DecrementResourceCount(Data.Domain, ResourceEnum.Geofence), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
        }

        public async Task HandleAsync(GeofenceDeleted message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            if (Data.Message.FrontendRequest)
            {
                busPublisher.Send(new SendPusherDomainUserCustomNotification("geofence-deleted", $"Geofence '{Data.Message.ExternalId}' was successfully deleted.", Data.Domain, Data.Initiator, OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
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
                    busPublisher.Send(new SendPusherDomainUserCustomNotification("geofence-deleted", $"An error occurred deleting geofence '{Data.Message.ExternalId}'. {message.Reason}", Data.Domain, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                }
                else
                {
                    busPublisher.Send(new SendPusherDomainUserCustomNotification("geofence-deleted", $"An error occurred deleting geofence '{Data.Message.ExternalId}'.", Data.Domain, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
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
                Data.Message.Domain,
                Data.Message.ExternalId,
                Data.Message.ProjectId
            );
            busPublisher.Send(deleteGeofence, CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public Task HandleAsync(DecrementResourceCountRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            logger.LogCritical($"Failed to decrement the Geofence utilization for tenant domain '{Data.Domain}'! Reason: {message.Reason}.");
            return RejectAsync();
        }
    }

    public class DeleteGeofenceData : BaseSagaData
    {
        public DeleteGeofenceSagaInitializer Message;
    }
}
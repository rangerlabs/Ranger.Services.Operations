using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chronicle;
using Microsoft.Extensions.Logging;
using Ranger.RabbitMQ;
using Ranger.RabbitMQ.BusPublisher;
using Ranger.Services.Operations.Data;
using Ranger.Services.Operations.Messages.Geofences;
using Ranger.Services.Operations.Messages.Operations.Commands;

namespace Ranger.Services.Operations.Sagas
{
    public class BulkDeleteGeofencesSaga : Saga<BulkDeleteGeofencesSagaData>,
        ISagaStartAction<BulkDeleteGeofencesSagaInitializer>,
        ISagaAction<GeofencesBulkDeleted>,
        ISagaAction<BulkDeleteGeofencesRejected>
    {
        private readonly ILogger<BulkDeleteGeofencesSaga> logger;
        private readonly IBusPublisher busPublisher;

        public BulkDeleteGeofencesSaga(IBusPublisher busPublisher, ILogger<BulkDeleteGeofencesSaga> logger)
        {
            this.busPublisher = busPublisher;
            this.logger = logger;
        }

        public Task CompensateAsync(BulkDeleteGeofencesSagaInitializer message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(GeofencesBulkDeleted message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(BulkDeleteGeofencesRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task HandleAsync(BulkDeleteGeofencesSagaInitializer message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            Data.FrontendRequest = message.FrontendRequest;
            Data.TenantId = message.TenantId;
            Data.Initiator = message.CommandingUserEmailOrTokenPrefix;
            Data.RequestedExternalIdsToDelete = message.ExternalIds;
            var bulkDeleteGeofence = new BulkDeleteGeofences(message.CommandingUserEmailOrTokenPrefix, message.TenantId, message.ExternalIds, message.ProjectId);
            busPublisher.Send(bulkDeleteGeofence, CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public async Task HandleAsync(GeofencesBulkDeleted message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            Data.DeletedIds = message.DeletedIds;
            if (Data.FrontendRequest)
            {
                busPublisher.Send(new SendPusherDomainUserCustomNotification("geofences-bulk-deleted", $"Successfully completed bulk delete operation", Data.TenantId, Data.Initiator, OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                await CompleteAsync();
            }
            else
            {
                await CompleteAsync();
            }
        }

        public async Task HandleAsync(BulkDeleteGeofencesRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            if (Data.FrontendRequest)
            {
                if (!string.IsNullOrWhiteSpace(message.Reason))
                {
                    busPublisher.Send(new SendPusherDomainUserCustomNotification("geofences-bulk-deleted", $"Failed to bulk delete geofences. { message.Reason }", Data.TenantId, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                }
                else
                {
                    busPublisher.Send(new SendPusherDomainUserCustomNotification("geofences-bulk-deleted", $"Failed to bulk delete geofences.", Data.TenantId, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                }
                await RejectAsync();
            }
            else
            {
                await RejectAsync();
            }
        }
    }

    public class BulkDeleteGeofencesSagaData : BaseSagaData
    {
        public bool FrontendRequest { get; set; }
        public IEnumerable<string> RequestedExternalIdsToDelete { get; set; }
        public IEnumerable<Guid> DeletedIds { get; set; }
    }
}
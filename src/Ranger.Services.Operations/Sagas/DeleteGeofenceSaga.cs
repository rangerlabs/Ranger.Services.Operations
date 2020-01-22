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
    public class DeleteGeofenceSaga : Saga<DeleteGeofenceData>,
        ISagaStartAction<DeleteGeofenceSagaInitializer>,
        ISagaAction<GeofenceDeleted>,
        ISagaAction<DeleteGeofenceRejected>
    {
        private readonly ILogger<DeleteGeofenceSaga> logger;
        private readonly IBusPublisher busPublisher;

        public DeleteGeofenceSaga(IBusPublisher busPublisher, ILogger<DeleteGeofenceSaga> logger)
        {
            this.busPublisher = busPublisher;
            this.logger = logger;
        }

        public async Task CompensateAsync(DeleteGeofenceSagaInitializer message, ISagaContext context)
        {
            logger.LogInformation("Calling compensate for DeleteGeofenceSagaInitializer.");
            await Task.CompletedTask;
        }

        public async Task CompensateAsync(GeofenceDeleted message, ISagaContext context)
        {
            logger.LogInformation("Calling compensate for GeofenceDeleted.");
            await Task.CompletedTask;
        }

        public async Task CompensateAsync(DeleteGeofenceRejected message, ISagaContext context)
        {
            logger.LogInformation("Calling compensate for DeleteGeofenceRejected.");
            await Task.CompletedTask;
        }

        public async Task HandleAsync(DeleteGeofenceSagaInitializer message, ISagaContext context)
        {
            await Task.Run(() =>
            {

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
            });
        }

        public async Task HandleAsync(GeofenceDeleted message, ISagaContext context)
        {
            logger.LogInformation("Calling handle for GeofenceDeleted.");
            if (Data.FrontendRequest)
            {
                busPublisher.Send(new SendPusherDomainUserCustomNotification("geofence-deleted", $"Geofence {Data.ExternalId} was successfully deleted.", Data.Domain, Data.Initiator, OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                await CompleteAsync();
            }
            else
            {
                await CompleteAsync();
            }
        }

        public async Task HandleAsync(DeleteGeofenceRejected message, ISagaContext context)
        {
            logger.LogInformation("Calling handle for DeleteGeofenceRejected.");
            if (Data.FrontendRequest)
            {
                if (!string.IsNullOrWhiteSpace(message.Reason))
                {
                    busPublisher.Send(new SendPusherDomainUserCustomNotification("geofence-deleted", $"An error occurred deleting the geofence {Data.ExternalId}. {message.Reason}", Data.Domain, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                }
                else
                {
                    busPublisher.Send(new SendPusherDomainUserCustomNotification("geofence-deleted", $"An error occurred deleting the geofence {Data.ExternalId}.", Data.Domain, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                }
                await RejectAsync();
            }
            else
            {
                await RejectAsync();
            }
        }
    }

    public class DeleteGeofenceData : BaseSagaData
    {
        public bool FrontendRequest;
        public string ExternalId;
    }
}
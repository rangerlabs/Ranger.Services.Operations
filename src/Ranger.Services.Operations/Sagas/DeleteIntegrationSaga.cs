using System;
using System.Threading.Tasks;
using Chronicle;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;
using Ranger.Services.Operations.Data;
using Ranger.Services.Operations.Messages.Geofences;
using Ranger.Services.Operations.Messages.Geofences.Events;
using Ranger.Services.Operations.Messages.Integrations;
using Ranger.Services.Operations.Messages.Operations;
using Ranger.Services.Operations.Messages.Subscriptions;

namespace Ranger.Services.Operations.Sagas
{
    public class DeleteIntegrationSaga : Saga<DeleteIntegrationData>,
        ISagaStartAction<DeleteIntegrationSagaInitializer>,
        ISagaAction<IntegrationDeleted>,
        ISagaAction<DeleteIntegrationRejected>,
        ISagaAction<IntegrationPurgedFromGeofences>
    {
        private readonly ILogger<DeleteIntegrationSaga> logger;
        private readonly IBusPublisher busPublisher;

        public DeleteIntegrationSaga(IBusPublisher busPublisher, ILogger<DeleteIntegrationSaga> logger)
        {
            this.busPublisher = busPublisher;
            this.logger = logger;
        }

        public Task CompensateAsync(DeleteIntegrationSagaInitializer message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(IntegrationDeleted message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(DeleteIntegrationRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(IntegrationPurgedFromGeofences message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task HandleAsync(DeleteIntegrationSagaInitializer message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            Data.TenantId = message.TenantId;
            Data.Initiator = message.CommandingUserEmail;
            Data.Message = message;
            var deleteIntegration = new DeleteIntegration(
               Data.Initiator,
               Data.Message.TenantId,
               Data.Message.Name,
               Data.Message.ProjectId
           );
            busPublisher.Send(deleteIntegration, CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public Task HandleAsync(IntegrationDeleted message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            busPublisher.Send(new PurgeIntegrationFromGeofences(Data.TenantId, Data.Message.ProjectId, message.IntegrationId), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public async Task HandleAsync(DeleteIntegrationRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            if (!string.IsNullOrWhiteSpace(message.Reason))
            {
                busPublisher.Send(new SendPusherDomainUserCustomNotification("integration-deleted", $"Failed to delete integration '{Data.Message.Name}'. {message.Reason}", Data.TenantId, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            }
            else
            {
                busPublisher.Send(new SendPusherDomainUserCustomNotification("integration-deleted", $"Failed to delete integration '{Data.Message.Name}'", Data.TenantId, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            }
            await RejectAsync();
        }

        public async Task HandleAsync(IntegrationPurgedFromGeofences message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            busPublisher.Send(new SendPusherDomainUserCustomNotification("integration-deleted", $"Successfully deleted integration '{Data.Message.Name}'", Data.TenantId, Data.Initiator, OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            await CompleteAsync();
        }
    }

    public class DeleteIntegrationData : BaseSagaData
    {
        public DeleteIntegrationSagaInitializer Message;
    }
}
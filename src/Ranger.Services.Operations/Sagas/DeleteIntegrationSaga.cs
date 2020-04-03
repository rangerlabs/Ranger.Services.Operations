using System;
using System.Threading.Tasks;
using Chronicle;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;
using Ranger.Services.Operations.Data;
using Ranger.Services.Operations.Messages.Integrations;
using Ranger.Services.Operations.Messages.Operations;
using Ranger.Services.Operations.Messages.Subscriptions;

namespace Ranger.Services.Operations.Sagas
{
    public class DeleteIntegrationSaga : BaseSaga<DeleteIntegrationSaga, DeleteIntegrationData>,
        ISagaStartAction<DeleteIntegrationSagaInitializer>,
        ISagaAction<IntegrationDeleted>,
        ISagaAction<ResourceCountDecremented>,
        ISagaAction<DeleteIntegrationRejected>,
        ISagaAction<DecrementResourceCountRejected>
    {
        private readonly ILogger<DeleteIntegrationSaga> logger;
        private readonly IBusPublisher busPublisher;
        private readonly ITenantsClient tenantsClient;

        public DeleteIntegrationSaga(IBusPublisher busPublisher, ITenantsClient tenantsClient, ILogger<DeleteIntegrationSaga> logger) : base(tenantsClient, logger)
        {
            this.tenantsClient = tenantsClient;
            this.busPublisher = busPublisher;
            this.logger = logger;
        }

        public Task CompensateAsync(DeleteIntegrationSagaInitializer message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(IntegrationDeleted message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(DeleteIntegrationRejected message, ISagaContext context)
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

        public async Task HandleAsync(DeleteIntegrationSagaInitializer message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            var databaseUsername = await GetPgsqlDatabaseUsernameOrReject(message);
            Data.DatabaseUsername = databaseUsername;
            Data.Domain = message.Domain;
            Data.Initiator = message.CommandingUserEmail;
            Data.Message = message;


            busPublisher.Send(new DecrementResourceCount(Data.Domain, ResourceEnum.Integration), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
        }

        public async Task HandleAsync(IntegrationDeleted message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            busPublisher.Send(new SendPusherDomainUserCustomNotification("integration-deleted", $"Integration '{Data.Message.Name}' was successfully deleted.", Data.Domain, Data.Initiator, OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            await CompleteAsync();
        }

        public async Task HandleAsync(DeleteIntegrationRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            if (!string.IsNullOrWhiteSpace(message.Reason))
            {
                busPublisher.Send(new SendPusherDomainUserCustomNotification("integration-deleted", $"An error occurred deleting integration '{Data.Message.Name}'. {message.Reason}", Data.Domain, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            }
            else
            {
                busPublisher.Send(new SendPusherDomainUserCustomNotification("integration-deleted", $"An error occurred deleting integration '{Data.Message.Name}'.", Data.Domain, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            }
            await RejectAsync();
        }

        public Task HandleAsync(ResourceCountDecremented message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            var deleteIntegration = new DeleteIntegration(
                Data.Initiator,
                Data.Message.Domain,
                Data.Message.Name,
                Data.Message.ProjectId
            );
            busPublisher.Send(deleteIntegration, CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public Task HandleAsync(DecrementResourceCountRejected message, ISagaContext context)
        {
            logger.LogCritical($"Failed to decrement the Integration utilization for tenant domain '{Data.Domain}'! Reason: {message.Reason}.");
            return Task.CompletedTask;
        }
    }

    public class DeleteIntegrationData : BaseSagaData
    {
        public DeleteIntegrationSagaInitializer Message;
    }
}
using System;
using System.Threading.Tasks;
using Chronicle;
using Microsoft.Extensions.Logging;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;
using Ranger.Services.Operations.Data;
using Ranger.Services.Operations.Messages.Integrations;
using Ranger.Services.Operations.Messages.Operations;

namespace Ranger.Services.Operations.Sagas
{
    public class DeleteIntegrationSaga : BaseSaga<DeleteIntegrationSaga, DeleteIntegrationData>,
        ISagaStartAction<DeleteIntegrationSagaInitializer>,
        ISagaAction<IntegrationDeleted>,
        ISagaAction<DeleteIntegrationRejected>
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

        public async Task HandleAsync(DeleteIntegrationSagaInitializer message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            var databaseUsername = await GetPgsqlDatabaseUsernameOrReject(message);
            Data.DatabaseUsername = databaseUsername;
            Data.Domain = message.Domain;
            Data.Initiator = message.CommandingUserEmail;
            Data.Name = message.Name;

            var deleteIntegration = new DeleteIntegration(
                message.CommandingUserEmail,
                message.Domain,
                message.Name,
                message.ProjectId
            );
            busPublisher.Send(deleteIntegration, CorrelationContext.FromId(Guid.Parse(context.SagaId)));
        }

        public async Task HandleAsync(IntegrationDeleted message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            busPublisher.Send(new SendPusherDomainUserCustomNotification("integration-deleted", $"Integration '{Data.Name}' was successfully deleted.", Data.Domain, Data.Initiator, OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            await CompleteAsync();
        }

        public async Task HandleAsync(DeleteIntegrationRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            if (!string.IsNullOrWhiteSpace(message.Reason))
            {
                busPublisher.Send(new SendPusherDomainUserCustomNotification("integration-deleted", $"An error occurred deleting integration '{Data.Name}'. {message.Reason}", Data.Domain, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            }
            else
            {
                busPublisher.Send(new SendPusherDomainUserCustomNotification("integration-deleted", $"An error occurred deleting integration '{Data.Name}'.", Data.Domain, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            }
            await RejectAsync();
        }
    }

    public class DeleteIntegrationData : BaseSagaData
    {
        public string Name;
    }
}
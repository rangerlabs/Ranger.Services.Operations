using System;
using System.Threading.Tasks;
using Chronicle;
using Microsoft.Extensions.Logging;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;
using Ranger.Services.Operations.Data;
using Ranger.Services.Operations.Messages.Integrations;
using Ranger.Services.Operations.Messages.Integrations.Commands;
using Ranger.Services.Operations.Messages.Operations;

namespace Ranger.Services.Operations.Sagas
{
    public class UpsertIntegrationSaga : BaseSaga<UpsertIntegrationSaga, UpsertIntegrationData>,
        ISagaStartAction<UpdateIntegrationSagaInitializer>,
        ISagaAction<IntegrationUpdated>,
        ISagaAction<UpdateIntegrationRejected>
    {
        private readonly ILogger<UpsertIntegrationSaga> logger;
        private readonly IBusPublisher busPublisher;
        private readonly ITenantsClient tenantsClient;

        public UpsertIntegrationSaga(IBusPublisher busPublisher, ITenantsClient tenantsClient, ILogger<UpsertIntegrationSaga> logger) : base(tenantsClient, logger)
        {
            this.tenantsClient = tenantsClient;
            this.busPublisher = busPublisher;
            this.logger = logger;
        }

        public async Task CompensateAsync(UpdateIntegrationSagaInitializer message, ISagaContext context)
        {
            logger.LogInformation("Calling compensate for UpsertIntegrationSagaInitializer.");
            await Task.CompletedTask;
        }

        public async Task CompensateAsync(IntegrationUpdated message, ISagaContext context)
        {
            logger.LogInformation("Calling compensate for IntegrationUpserted.");
            await Task.CompletedTask;
        }

        public async Task CompensateAsync(UpdateIntegrationRejected message, ISagaContext context)
        {
            logger.LogInformation("Calling compensate for UpsertIntegrationRejected.");
            await Task.CompletedTask;
        }

        public async Task HandleAsync(UpdateIntegrationSagaInitializer message, ISagaContext context)
        {
            var databaseUsername = await GetPgsqlDatabaseUsernameOrReject(message);
            Data.DatabaseUsername = databaseUsername;
            Data.Domain = message.Domain;
            Data.Initiator = message.CommandingUserEmail;
            Data.Name = message.Name;

            var updateIntegration = new UpdateIntegration(message.Domain, message.CommandingUserEmail, message.ProjectId, message.MessageJsonContent, message.IntegrationType, message.Version);
            busPublisher.Send(updateIntegration, CorrelationContext.FromId(Guid.Parse(context.SagaId)));
        }

        public async Task HandleAsync(IntegrationUpdated message, ISagaContext context)
        {
            logger.LogInformation("Calling handle for IntegrationUpdated.");
            busPublisher.Send(new SendPusherDomainUserCustomNotification("integration-updated", $"Integration '{Data.Name}' was successfully updated.", Data.Domain, Data.Initiator, OperationsStateEnum.Completed, message.Id), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            await CompleteAsync();

        }

        public async Task HandleAsync(UpdateIntegrationRejected message, ISagaContext context)
        {
            logger.LogInformation("Calling handle for UpdateIntegrationRejected.");
            if (!string.IsNullOrWhiteSpace(message.Reason))
            {
                busPublisher.Send(new SendPusherDomainUserCustomNotification("integration-updated", $"An error occurred updating integration '{Data.Name}'. {message.Reason}", Data.Domain, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            }
            else
            {
                busPublisher.Send(new SendPusherDomainUserCustomNotification("integration-updated", $"An error occurred updating integration '{Data.Name}'.", Data.Domain, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            }
            await RejectAsync();
        }
    }

    public class UpsertIntegrationData : BaseSagaData
    {
        public string Name;
    }
}
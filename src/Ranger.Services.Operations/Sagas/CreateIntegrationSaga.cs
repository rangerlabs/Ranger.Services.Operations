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

namespace Ranger.Services.Operations
{
    public class CreateIntegrationSaga : BaseSaga<CreateIntegrationSaga, CreateIntegrationData>,
        ISagaStartAction<CreateIntegrationSagaInitializer>,
        ISagaAction<IntegrationCreated>,
        ISagaAction<CreateIntegrationRejected>
    {
        private readonly ILogger<CreateIntegrationSaga> logger;
        private readonly IBusPublisher busPublisher;
        private readonly ITenantsClient tenantsClient;

        public CreateIntegrationSaga(IBusPublisher busPublisher, ITenantsClient tenantsClient, ILogger<CreateIntegrationSaga> logger) : base(tenantsClient, logger)
        {
            this.tenantsClient = tenantsClient;
            this.busPublisher = busPublisher;
            this.logger = logger;
        }

        public Task CompensateAsync(CreateIntegrationSagaInitializer message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(IntegrationCreated message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(CreateIntegrationRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            return Task.CompletedTask;
        }

        public async Task HandleAsync(CreateIntegrationSagaInitializer message, ISagaContext context)
        {
            var databaseUsername = await GetPgsqlDatabaseUsernameOrReject(message);
            Data.DatabaseUsername = databaseUsername;
            Data.Domain = message.Domain;
            Data.Initiator = message.CommandingUserEmail;
            Data.Name = message.Name;

            var createIntegration = new CreateIntegration(message.Domain, message.CommandingUserEmail, message.ProjectId, message.MessageJsonContent, message.IntegrationType);
            busPublisher.Send(createIntegration, CorrelationContext.FromId(Guid.Parse(context.SagaId)));
        }

        public async Task HandleAsync(IntegrationCreated message, ISagaContext context)
        {
            logger.LogInformation("Calling handle for IntegrationCreated.");
            busPublisher.Send(new SendPusherDomainUserCustomNotification("integration-created", $"Integration '{Data.Name}' was successfully created.", Data.Domain, Data.Initiator, OperationsStateEnum.Completed, message.Id), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            await CompleteAsync();
        }

        public async Task HandleAsync(CreateIntegrationRejected message, ISagaContext context)
        {
            logger.LogInformation("Calling handle for CreateIntegrationRejected.");
            if (!string.IsNullOrWhiteSpace(message.Reason))
            {
                busPublisher.Send(new SendPusherDomainUserCustomNotification("integration-created", $"An error occurred creating integration '{Data.Name}'. {message.Reason}", Data.Domain, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            }
            else
            {
                busPublisher.Send(new SendPusherDomainUserCustomNotification("integration-created", $"An error occurred creating integration '{Data.Name}'.", Data.Domain, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            }
            await RejectAsync();
        }
    }

    public class CreateIntegrationData : BaseSagaData
    {
        public string Name;
    }
}
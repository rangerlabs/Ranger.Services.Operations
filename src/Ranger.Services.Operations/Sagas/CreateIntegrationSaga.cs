using System;
using System.Threading.Tasks;
using Chronicle;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;
using Ranger.RabbitMQ.BusPublisher;
using Ranger.Services.Operations.Data;
using Ranger.Services.Operations.Messages.Integrations;
using Ranger.Services.Operations.Messages.Integrations.Commands;
using Ranger.Services.Operations.Messages.Operations;
using Ranger.Services.Operations.Messages.Subscriptions;

namespace Ranger.Services.Operations
{
    public class CreateIntegrationSaga : Saga<CreateIntegrationData>,
        ISagaStartAction<CreateIntegrationSagaInitializer>,
        ISagaAction<IntegrationCreated>,
        ISagaAction<CreateIntegrationRejected>
    {
        private readonly ILogger<CreateIntegrationSaga> logger;
        private readonly IBusPublisher busPublisher;

        public CreateIntegrationSaga(IBusPublisher busPublisher, ILogger<CreateIntegrationSaga> logger)
        {
            this.busPublisher = busPublisher;
            this.logger = logger;
        }

        public Task CompensateAsync(CreateIntegrationSagaInitializer message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(IntegrationCreated message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            busPublisher.Send(new DeleteIntegration("Operations", Data.TenantId, Data.Message.Name, Data.Message.ProjectId), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public Task CompensateAsync(CreateIntegrationRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task HandleAsync(CreateIntegrationSagaInitializer message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            Data.Message = message;
            Data.TenantId = message.TenantId;
            Data.Initiator = message.CommandingUserEmail;
            var createIntegration = new CreateIntegration(Data.TenantId, Data.Message.CommandingUserEmail, Data.Message.ProjectId, Data.Message.MessageJsonContent, Data.Message.IntegrationType);
            busPublisher.Send(createIntegration, CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public async Task HandleAsync(IntegrationCreated message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            Data.Id = message.Id;
            busPublisher.Send(new SendPusherDomainUserCustomNotification("integration-created", $"Successfully created integration '{Data.Message.Name}'", Data.TenantId, Data.Initiator, OperationsStateEnum.Completed, Data.Id), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            await CompleteAsync();
        }

        public async Task HandleAsync(CreateIntegrationRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            if (!string.IsNullOrWhiteSpace(message.Reason))
            {
                busPublisher.Send(new SendPusherDomainUserCustomNotification("integration-created", $"Failed to create integration '{Data.Message.Name}'. {message.Reason}", Data.TenantId, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            }
            else
            {
                busPublisher.Send(new SendPusherDomainUserCustomNotification("integration-created", $"Failed to create integration '{Data.Message.Name}'", Data.TenantId, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            }
            await RejectAsync();
        }
    }

    public class CreateIntegrationData : BaseSagaData, IResourceSagaData
    {
        public CreateIntegrationSagaInitializer Message;
        public Guid Id { get; set; }
        public string Name;
    }
}
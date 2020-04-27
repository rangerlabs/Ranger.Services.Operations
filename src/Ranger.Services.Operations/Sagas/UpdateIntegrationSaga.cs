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

namespace Ranger.Services.Operations.Sagas {
    public class UpsertIntegrationSaga : Saga<UpsertIntegrationData>,
        ISagaStartAction<UpdateIntegrationSagaInitializer>,
        ISagaAction<IntegrationUpdated>,
        ISagaAction<UpdateIntegrationRejected> {
            private readonly ILogger<UpsertIntegrationSaga> logger;
            private readonly IBusPublisher busPublisher;

            public UpsertIntegrationSaga (IBusPublisher busPublisher, ILogger<UpsertIntegrationSaga> logger) {
                this.busPublisher = busPublisher;
                this.logger = logger;
            }

            public Task CompensateAsync (UpdateIntegrationSagaInitializer message, ISagaContext context) {
                logger.LogDebug ($"Calling compensate for message '{message.GetType()}'");
                return Task.CompletedTask;
            }

            public Task CompensateAsync (IntegrationUpdated message, ISagaContext context) {
                logger.LogDebug ($"Calling compensate for message '{message.GetType()}'");
                return Task.CompletedTask;
            }

            public Task CompensateAsync (UpdateIntegrationRejected message, ISagaContext context) {
                logger.LogDebug ($"Calling compensate for message '{message.GetType()}'");
                return Task.CompletedTask;
            }

            public Task HandleAsync (UpdateIntegrationSagaInitializer message, ISagaContext context) {
                logger.LogDebug ($"Calling handle for message '{message.GetType()}'");
                Data.TenantId = message.TenantId;
                Data.Initiator = message.CommandingUserEmail;
                Data.Name = message.Name;

                var updateIntegration = new UpdateIntegration (message.TenantId, message.CommandingUserEmail, message.ProjectId, message.MessageJsonContent, message.IntegrationType, message.Version);
                busPublisher.Send (updateIntegration, CorrelationContext.FromId (Guid.Parse (context.SagaId)));
                return Task.CompletedTask;
            }

            public async Task HandleAsync (IntegrationUpdated message, ISagaContext context) {
                logger.LogDebug ($"Calling handle for message '{message.GetType()}'");
                busPublisher.Send (new SendPusherDomainUserCustomNotification ("integration-updated", $"Successfully updated integration '{Data.Name}'", Data.TenantId, Data.Initiator, OperationsStateEnum.Completed, message.Id), CorrelationContext.FromId (Guid.Parse (context.SagaId)));
                await CompleteAsync ();

            }

            public async Task HandleAsync (UpdateIntegrationRejected message, ISagaContext context) {
                logger.LogDebug ($"Calling handle for message '{message.GetType()}'");
                if (!string.IsNullOrWhiteSpace (message.Reason)) {
                    busPublisher.Send (new SendPusherDomainUserCustomNotification ("integration-updated", $"An error occurred updating integration '{Data.Name}'. {message.Reason}", Data.TenantId, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId (Guid.Parse (context.SagaId)));
                } else {
                    busPublisher.Send (new SendPusherDomainUserCustomNotification ("integration-updated", $"An error occurred updating integration '{Data.Name}'", Data.TenantId, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId (Guid.Parse (context.SagaId)));
                }
                await RejectAsync ();
            }
        }

    public class UpsertIntegrationData : BaseSagaData {
        public string Name;
    }
}
using System;
using System.Threading.Tasks;
using Chronicle;
using Microsoft.Extensions.Logging;
using Ranger.RabbitMQ;
using Ranger.Services.Operations.Data;
using Ranger.Services.Operations.Messages.Notifications.Commands.Email;
using Ranger.Services.Operations.Messages.Subscriptions.Commands;
using Ranger.Services.Operations.Messages.Subscriptions.Events;
using Ranger.Services.Operations.Messages.Subscriptions.RejectedEvents;
using Ranger.Services.Operations.Messages.Tenants.Events;
using Ranger.Services.Operations.Messages.Tenants.RejectedEvents;

namespace Ranger.Services.Operations.Sagas
{
    public class UpdateTenantOrganizationNameSaga : Saga<UpdateTenantOrganizationData>,
        ISagaStartAction<UpdateTenantOrganizationSagaInitializer>,
        ISagaAction<TenantOrganizationUpdated>,
        ISagaAction<UpdateTenantOrganizationRejected>,
        ISagaAction<TenantSubscriptionOrganizationUpdated>,
        ISagaAction<UpdateTenantSubscriptionOrganizationRejected>
    {
        private readonly ILogger<UpsertIntegrationSaga> logger;
        private readonly IBusPublisher busPublisher;

        public UpdateTenantOrganizationNameSaga(IBusPublisher busPublisher, ILogger<UpsertIntegrationSaga> logger)
        {
            this.busPublisher = busPublisher;
            this.logger = logger;
        }

        public Task CompensateAsync(UpdateTenantOrganizationSagaInitializer message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(TenantOrganizationUpdated message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(UpdateTenantOrganizationRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(TenantSubscriptionOrganizationUpdated message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(UpdateTenantSubscriptionOrganizationRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task HandleAsync(UpdateTenantOrganizationSagaInitializer message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            Data.TenantId = message.TenantId;
            Data.Initiator = message.CommandingUserEmail;
            Data.OrganizationName = message.OrganizationName;
            Data.Domain = message.Domain;

            var updateIntegration = new UpdateTenantOrganization(message.TenantId, message.CommandingUserEmail, message.Version, message.OrganizationName, message.Domain);
            busPublisher.Send(updateIntegration, CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public Task HandleAsync(TenantOrganizationUpdated message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            Data.DomainWasUpdated = message.DomainWasUpdated;
            busPublisher.Send(new UpdateTenantSubscriptionOrganization(Data.TenantId, message.OrganizationName, message.Domain), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public async Task HandleAsync(UpdateTenantOrganizationRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            if (!string.IsNullOrWhiteSpace(message.Reason))
            {
                busPublisher.Send(new SendPusherDomainUserCustomNotification("organization-updated", $"Failed to update organization. {message.Reason}", Data.TenantId, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            }
            else
            {
                busPublisher.Send(new SendPusherDomainUserCustomNotification("organization-updated", $"Failed to update organization", Data.TenantId, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            }
            await RejectAsync();
        }

        public Task HandleAsync(TenantSubscriptionOrganizationUpdated message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            SendOrganizationUpdatedNotifications(context);
            Complete();
            return Task.CompletedTask;
        }

        public Task HandleAsync(UpdateTenantSubscriptionOrganizationRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            logger.LogError("Failed to update the Chargebee subscription organization for TenantId {TenantId}. Expected the new domain to be {Domain} and the new Organization Name to be {OrganizationName}", Data.TenantId, Data.Domain, Data.OrganizationName);
            SendOrganizationUpdatedNotifications(context);
            Complete();
            return Task.CompletedTask;
        }

        private void SendOrganizationUpdatedNotifications(ISagaContext context)
        {
            busPublisher.Send(new SendPusherDomainCustomNotification("organization-updated", $"Successfully updated your organization details", Data.TenantId), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            if (Data.DomainWasUpdated)
            {
                busPublisher.Send(new SendTenantDomainUpdatedEmails(Data.TenantId, Data.Domain, Data.Initiator), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            }
        }
    }

    public class UpdateTenantOrganizationData : BaseSagaData
    {
        public string OrganizationName;
        public string Domain;
        public bool DomainWasUpdated;
    }
}
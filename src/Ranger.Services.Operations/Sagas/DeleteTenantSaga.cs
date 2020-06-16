using System;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using Chronicle;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;
using Ranger.Services.Operations.Messages.Notifications;
using Ranger.Services.Operations.Messages.Operations;
using Ranger.Services.Operations.Messages.Subscriptions.Commands;
using Ranger.Services.Operations.Messages.Subscriptions.Events;
using Ranger.Services.Operations.Messages.Subscriptions.RejectedEvents;
using Ranger.Services.Operations.Messages.Tenants.Commands;
using Ranger.Services.Operations.Messages.Tenants.Events;
using Ranger.Services.Operations.Messages.Tenants.RejectedEvents;

namespace Ranger.Services.Operations.Sagas
{
    public class DeleteTenantSaga : Saga<DeleteTenantData>,
        ISagaStartAction<DeleteTenantSagaInitializer>,
        ISagaAction<TenantSubscriptionCancelled>,
        ISagaAction<CancelTenantSubscriptionRejected>,
        ISagaAction<TenantDeleted>,
        ISagaAction<DeleteTenantRejected>
    {
        private readonly IBusPublisher busPublisher;
        private readonly ILogger<DeleteTenantSaga> logger;
        private readonly IdentityHttpClient identityClient;

        public DeleteTenantSaga(IBusPublisher busPublisher, IdentityHttpClient identityClient, ILogger<DeleteTenantSaga> logger)
        {
            this.identityClient = identityClient;
            this.busPublisher = busPublisher;
            this.logger = logger;
        }

        public Task CompensateAsync(DeleteTenantSagaInitializer message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(TenantDeleted message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(DeleteTenantRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(TenantSubscriptionCancelled message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(CancelTenantSubscriptionRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        //TODO: How to handle retries in these handlers?
        public Task HandleAsync(DeleteTenantSagaInitializer message, ISagaContext context)
        {
            Data.TenantId = message.TenantId;
            Data.Initiator = message.CommandingUserEmail;
            busPublisher.Send(new CancelTenantSubscription(message.TenantId), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public async Task HandleAsync(TenantSubscriptionCancelled message, ISagaContext context)
        {
            try
            {
                RangerApiResponse<User> apiResponse = null;
                try
                {
                    apiResponse = await identityClient.GetUserAsync<User>(message.TenantId, Data.Initiator);
                }
                catch (ApiException ex)
                {
                    logger.LogError(ex, "Failed to retrieve the Primary Owner when attempting Primary Ownership transfer");
                    await RejectAsync();
                }
                Data.OwnerUser = apiResponse.Result;
                Data.Initiator = Data.Initiator;
                Data.TenantId = message.TenantId;
                busPublisher.Send(new DeleteTenant(Data.Initiator, message.TenantId), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            }
            catch (ApiException)
            {
                logger.LogError("Failed to retrieve the Primary Owner when attempting Primary Ownership transfer");
                await RejectAsync();
            }
        }

        public Task HandleAsync(TenantDeleted message, ISagaContext context)
        {
            busPublisher.Send(new SendDomainDeletedEmail(Data.Initiator, Data.OwnerUser.FirstName, message.OrganizationName), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public Task HandleAsync(DeleteTenantRejected message, ISagaContext context)
        {
            logger.LogError("Failed to delete tenant with Tenant Id {TenantID}", Data.TenantId);
            return Task.CompletedTask;
        }

        public Task HandleAsync(CancelTenantSubscriptionRejected message, ISagaContext context)
        {
            logger.LogError("Failed to cancel tenant subscription for Tenant Id {TenantID}", Data.TenantId);
            return Task.CompletedTask;
        }
    }

    public class DeleteTenantData : BaseSagaData
    {
        public User OwnerUser { get; set; }
    }
}
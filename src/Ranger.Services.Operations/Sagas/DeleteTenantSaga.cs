using System;
using System.Threading.Tasks;
using Chronicle;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;
using Ranger.Services.Operations.Messages.Notifications;
using Ranger.Services.Operations.Messages.Operations;
using Ranger.Services.Operations.Messages.Tenants.Commands;
using Ranger.Services.Operations.Messages.Tenants.Events;
using Ranger.Services.Operations.Messages.Tenants.RejectedEvents;

namespace Ranger.Services.Operations.Sagas
{
    public class DeleteTenantSaga : Saga<DeleteTenantData>,
        ISagaStartAction<DeleteTenantSagaInitializer>,
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
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(TenantDeleted message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(DeleteTenantRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            return Task.CompletedTask;
        }

        public async Task HandleAsync(DeleteTenantSagaInitializer message, ISagaContext context)
        {
            Data.TenantId = message.TenantId;
            var apiResponse = await identityClient.GetUserAsync<User>(message.TenantId, message.CommandingUserEmail);
            if (apiResponse.IsError)
            {
                logger.LogError("Failed to retrieve the Primary Owner when attempting Primary Ownership transfer.");
                await RejectAsync();
                Data.OwnerUser = apiResponse.Result;
            }
            Data.Initiator = message.CommandingUserEmail;
            Data.TenantId = message.TenantId;
            busPublisher.Send(new DeleteTenant(message.CommandingUserEmail, message.TenantId), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
        }

        public Task HandleAsync(TenantDeleted message, ISagaContext context)
        {
            busPublisher.Send(new SendDomainDeletedEmail(Data.Initiator, Data.OwnerUser.FirstName, message.OrganizationName), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public Task HandleAsync(DeleteTenantRejected message, ISagaContext context)
        {
            logger.LogError($"Failed to delete tenant domain '{Data.TenantId}'.");
            return Task.CompletedTask;
        }
    }

    public class DeleteTenantData : BaseSagaData
    {
        public User OwnerUser { get; set; }
    }
}
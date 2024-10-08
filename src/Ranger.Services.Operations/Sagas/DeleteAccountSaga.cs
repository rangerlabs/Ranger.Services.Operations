using System;
using System.Threading.Tasks;
using Chronicle;
using Microsoft.Extensions.Logging;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;
using Ranger.RabbitMQ.BusPublisher;
using Ranger.Services.Operations.Messages.Projects;

namespace Ranger.Services.Operations
{
    public class DeleteAccountSaga : Saga<DeleteAccountData>,
        ISagaStartAction<DeleteAccountSagaInitializer>,
        ISagaAction<AccountDeleted>,
        ISagaAction<DeleteAccountRejected>,
        ISagaAction<UserProjectsUpdated>
    {
        const string EVENT_NAME = "account-deleted";
        private readonly IBusPublisher busPublisher;
        private readonly ILogger<CreateUserSaga> logger;
        private readonly IIdentityHttpClient identityClient;

        public DeleteAccountSaga(IBusPublisher busPublisher, IIdentityHttpClient identityClient, ILogger<CreateUserSaga> logger)
        {
            this.identityClient = identityClient;
            this.logger = logger;
            this.busPublisher = busPublisher;
        }

        public Task CompensateAsync(DeleteAccountSagaInitializer message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(AccountDeleted message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(DeleteAccountRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(UserProjectsUpdated message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task HandleAsync(DeleteAccountSagaInitializer message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            Data.Email = message.Email;
            Data.Initiator = message.Email;
            Data.TenantId = message.TenantId;
            busPublisher.Send(new DeleteAccount(message.TenantId, message.Email, message.Password), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;

        }

        public Task HandleAsync(AccountDeleted message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            busPublisher.Send(new UpdateUserProjects(Data.TenantId, new Guid[0], message.UserId, message.Email, message.Email), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            busPublisher.Send(new SendPusherDomainUserCustomNotification(EVENT_NAME, $"Successfully deleted account", Data.TenantId, Data.Initiator, Operations.Data.OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public Task HandleAsync(DeleteAccountRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            busPublisher.Send(new SendPusherDomainUserCustomNotification(EVENT_NAME, $"Failed to delete account: {message.Reason}", Data.TenantId, Data.Initiator, Operations.Data.OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            Reject();
            return Task.CompletedTask;
        }

        public Task HandleAsync(UserProjectsUpdated message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            Complete();
            return Task.CompletedTask;
        }
    }

    public class DeleteAccountData : BaseSagaData
    {
        public string Email { get; set; }
    }
}
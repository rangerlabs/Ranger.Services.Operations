using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using Chronicle;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;
using Ranger.Services.Operations.Data;
using Ranger.Services.Operations.Messages.Notifications;
using Ranger.Services.Operations.Messages.Operations;
using Ranger.Services.Operations.Messages.Projects;
using Ranger.Services.Operations.Messages.Subscriptions;

namespace Ranger.Services.Operations
{
    public class DeleteUserSaga : Saga<DeleteUserData>,
        ISagaStartAction<DeleteUserSagaInitializer>,
        ISagaAction<UserDeleted>,
        ISagaAction<DeleteUserRejected>,
        ISagaAction<UserProjectsUpdated>
    {
        const string EVENT_NAME = "user-deleted";
        private readonly IBusPublisher busPublisher;
        private readonly ILogger<CreateUserSaga> logger;
        private readonly IdentityHttpClient identityClient;

        public DeleteUserSaga(IBusPublisher busPublisher, IdentityHttpClient identityClient, ILogger<CreateUserSaga> logger)
        {
            this.identityClient = identityClient;
            this.logger = logger;
            this.busPublisher = busPublisher;
        }

        public Task CompensateAsync(DeleteUserSagaInitializer message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(UserDeleted message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(DeleteUserRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(UserProjectsUpdated message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task HandleAsync(DeleteUserSagaInitializer message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            Data.Email = message.Email;
            Data.Initiator = message.Email;
            Data.TenantId = message.TenantId;
            busPublisher.Send(new DeleteUser(message.TenantId, message.Email, message.CommandingUserEmail), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public Task HandleAsync(UserDeleted message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            busPublisher.Send(new UpdateUserProjects(Data.TenantId, new Guid[0], message.UserId, message.Email, Data.Initiator), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public Task HandleAsync(DeleteUserRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            busPublisher.Send(new SendPusherDomainUserCustomNotification(EVENT_NAME, $"Failed to delete user {Data.Email}: {message.Reason}", Data.TenantId, Data.Initiator, Operations.Data.OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            Reject();
            return Task.CompletedTask;
        }

        public Task HandleAsync(UserProjectsUpdated message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            busPublisher.Send(new SendPusherDomainUserPredefinedNotification("ForceSignoutNotification", Data.TenantId, Data.Email), CorrelationContext.Empty);
            busPublisher.Send(new SendPusherDomainUserCustomNotification(EVENT_NAME, $"Successfully deleted user {Data.Email}", Data.TenantId, Data.Initiator, Operations.Data.OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            Complete();
            return Task.CompletedTask;
        }
    }

    public class DeleteUserData : BaseSagaData
    {
        public string Email { get; set; }
    }
}
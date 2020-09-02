using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chronicle;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;
using Ranger.RabbitMQ.BusPublisher;
using Ranger.Services.Operations.Data;
using Ranger.Services.Operations.Messages.Notifications;
using Ranger.Services.Operations.Messages.Operations;
using Ranger.Services.Operations.Messages.Projects;

namespace Ranger.Services.Operations
{
    public class CreateUserSaga : Saga<CreateUserData>,
        ISagaStartAction<CreateUserSagaInitializer>,
        ISagaAction<UserCreated>,
        ISagaAction<CreateUserRejected>,
        ISagaAction<SendNewUserEmailSent>,
        ISagaAction<UserProjectsUpdated>,
        ISagaAction<UpdateUserProjectsRejected>
    {
        const string EVENT_NAME = "user-created";
        private readonly IBusPublisher busPublisher;
        private readonly ILogger<CreateUserSaga> logger;
        private readonly ProjectsHttpClient projectsClient;
        private readonly IdentityHttpClient identityClient;

        public CreateUserSaga(IBusPublisher busPublisher, IdentityHttpClient identityClient, ProjectsHttpClient projectsClient, ILogger<CreateUserSaga> logger)
        {
            this.identityClient = identityClient;
            this.projectsClient = projectsClient;
            this.logger = logger;
            this.busPublisher = busPublisher;
        }

        public Task CompensateAsync(UserCreated message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(SendNewUserEmailSent message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(CreateUserSagaInitializer message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(CreateUserRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(UpdateUserProjectsRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(UserProjectsUpdated message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public async Task HandleAsync(UpdateUserProjectsRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            var notificationText = $"Successfully created {Data.Message.Email} but failed to set their authorized projects";
            busPublisher.Send(new SendPusherDomainUserCustomNotification(EVENT_NAME, notificationText, Data.TenantId, Data.Initiator, Operations.Data.OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            await CompleteAsync();
        }

        public async Task HandleAsync(UserProjectsUpdated message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            var notificationText = "";
            if (message.UnSuccessfullyAddedProjectIds.Count() > 0)
            {
                notificationText = $"Successfully created {Data.Message.Email} but some projects failed be added. Verify the selected projects and try again";
            }
            else
            {
                notificationText = $"User {Data.Message.Email} was successfully created";
            }

            await SendNewUserEmail(context);
        }

        public async Task HandleAsync(UserCreated message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            Data.UserId = message.UserId;
            Data.Token = message.Token;

            if (Data.NewRole != RolesEnum.User)
            {
                await SendNewUserEmail(context);
            }
            else
            {
                if (Data.Message.AuthorizedProjects.Count() > 0)
                {
                    busPublisher.Send(new UpdateUserProjects(Data.TenantId, Data.Message.AuthorizedProjects, Data.UserId, Data.Message.Email, Data.Initiator), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                }
                else
                {
                    await SendNewUserEmail(context);
                }
            }
        }

        public async Task HandleAsync(SendNewUserEmailSent message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            busPublisher.Send(new SendPusherDomainUserCustomNotification(EVENT_NAME, $"Successfully created user {Data.Message.Email}", Data.TenantId, Data.Initiator, Operations.Data.OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            await CompleteAsync();
        }

        public Task HandleAsync(CreateUserSagaInitializer message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");

            Data.TenantId = message.TenantId;
            Data.Message = message;
            Data.Initiator = message.CommandingUserEmail;
            Data.NewRole = message.Role;
            var createNewUser = new CreateUser(
                         Data.TenantId,
                         Data.Message.Email,
                         Data.Message.FirstName,
                         Data.Message.LastName,
                         Data.Message.Role,
                         Data.Message.CommandingUserEmail,
                         Data.Message.AuthorizedProjects
                     );
            busPublisher.Send(createNewUser, CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public async Task HandleAsync(CreateUserRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            busPublisher.Send(new SendPusherDomainUserCustomNotification(EVENT_NAME, $"Failed to create user '{Data.Message.Email}'. {message.Reason}", Data.TenantId, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            await RejectAsync();
        }

        private async Task SendNewUserEmail(ISagaContext context)
        {
            IEnumerable<string> authorizedProjectNames = await Utilities.GetProjectNamesForAuthorizedProjectsAsync(Data.TenantId, Data.Message.Email, Data.NewRole, Data.Message.AuthorizedProjects, projectsClient).ConfigureAwait(false);

            await Task.Run(() =>
            {
                var sendNewUserEmail = new SendNewUserEmail(
                    Data.UserId,
                    Data.Message.Email,
                    Data.Message.FirstName,
                    Data.TenantId,
                    Enum.GetName(typeof(RolesEnum), Data.NewRole),
                    Data.Token,
                    authorizedProjectNames
                );
                this.busPublisher.Send(sendNewUserEmail, CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            });
        }
    }

    public class CreateUserData : BaseSagaData
    {
        public string UserId { get; set; }
        public string Token { get; set; }
        public RolesEnum NewRole { get; set; }
        public CreateUserSagaInitializer Message { get; set; }
    }
}
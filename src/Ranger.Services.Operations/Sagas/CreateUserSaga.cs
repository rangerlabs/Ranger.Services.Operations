using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chronicle;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;
using Ranger.Services.Operations.Messages.Notifications;
using Ranger.Services.Operations.Messages.Operations;
using Ranger.Services.Operations.Messages.Projects;
using Ranger.Services.Operations.Messages.Subscriptions;

namespace Ranger.Services.Operations
{
    public class CreateUserSaga : BaseSaga<CreateUserSaga, CreateUserData>,
        ISagaStartAction<CreateUserSagaInitializer>,
        ISagaAction<UserCreated>,
        ISagaAction<CreateUserRejected>,
        ISagaAction<SendNewUserEmailSent>,
        ISagaAction<UserProjectsUpdated>,
        ISagaAction<ResourceCountIncremented>,
        ISagaAction<UpdateUserProjectsRejected>,
        ISagaAction<IncrementResourceCountRejected>

    {
        const string EVENT_NAME = "user-created";
        private readonly IBusPublisher busPublisher;
        private readonly ILogger<CreateUserSaga> logger;
        private readonly IProjectsClient projectsClient;
        private readonly ITenantsClient tenantsClient;
        private readonly IIdentityClient identityClient;

        public CreateUserSaga(IBusPublisher busPublisher, IIdentityClient identityClient, IProjectsClient projectsClient, ITenantsClient tenantsClient, ILogger<CreateUserSaga> logger) : base(tenantsClient, logger)
        {
            this.identityClient = identityClient;
            this.projectsClient = projectsClient;
            this.tenantsClient = tenantsClient;
            this.logger = logger;
            this.busPublisher = busPublisher;
        }

        public async Task CompensateAsync(UserCreated message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            try
            {
                var deleteUserContent = new { CommandingUserEmail = Data.Initiator };
                await identityClient.DeleteUserAsync(Data.Domain, Data.UserEmail, JsonConvert.SerializeObject(deleteUserContent));
            }
            catch (HttpClientException ex)
            {
                logger.LogError(ex, $"Failed to remove user '{Data.UserEmail}' after a Saga failure.");
            }
            logger.LogDebug($"Successfully removed user '{Data.UserEmail}' after a Saga failure.");
        }

        public Task CompensateAsync(SendNewUserEmailSent message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            return Task.CompletedTask;
        }

        public async Task CompensateAsync(CreateUserSagaInitializer message, ISagaContext context)
        {
            await Task.Run(() =>
            {
                logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
                busPublisher.Send(new SendPusherDomainUserCustomNotification(EVENT_NAME, $"Error creating user {Data.UserEmail}: {Data.RejectReason}", Data.Domain, Data.Initiator, Operations.Data.OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            });
        }

        public Task CompensateAsync(CreateUserRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(UpdateUserProjectsRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(UserProjectsUpdated message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(ResourceCountIncremented message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(IncrementResourceCountRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            return Task.CompletedTask;
        }

        public async Task HandleAsync(UpdateUserProjectsRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            var notificationText = $"Successfully created {Data.UserEmail} but failed to set their authorized projects. Verify the selected projects and try again.";
            busPublisher.Send(new SendPusherDomainUserCustomNotification(EVENT_NAME, notificationText, Data.Domain, Data.Initiator, Operations.Data.OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            await CompleteAsync();
        }

        public async Task HandleAsync(UserProjectsUpdated message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            var notificationText = "";
            if (message.UnSuccessfullyAddedProjectIds.Count() > 0)
            {
                notificationText = $"Successfully created {Data.UserEmail} but some projects failed be added. Verify the selected projects and try again.";
            }
            else
            {
                notificationText = $"User {Data.UserEmail} was successfully created.";
            }

            await SendNewUserEmail(context);
        }

        public Task HandleAsync(UserCreated message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            Data.UserId = message.UserId;
            Data.FirstName = message.FirstName;
            Data.Token = message.Token;

            busPublisher.Send(new IncrementResourceCount(Data.Domain, ResourceEnum.Account), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public async Task HandleAsync(SendNewUserEmailSent message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            busPublisher.Send(new SendPusherDomainUserCustomNotification(EVENT_NAME, $"User {Data.UserEmail} was succesfully created.", Data.Domain, Data.Initiator, Operations.Data.OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            await CompleteAsync();
        }

        public async Task HandleAsync(CreateUserSagaInitializer message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            var databaseUsername = await GetPgsqlDatabaseUsernameOrReject(message);
            Data.DatabaseUsername = databaseUsername;
            Data.Domain = message.Domain;
            Data.UserEmail = message.Email;
            Data.Initiator = message.CommandingUserEmail;
            Data.NewAuthorizedProjects = message.AuthorizedProjects;
            Data.NewRole = Enum.Parse<RolesEnum>(message.Role);

            var createNewUser = new CreateUser(
                message.Domain,
                message.Email,
                message.FirstName,
                message.LastName,
                message.Role,
                message.CommandingUserEmail,
                message.AuthorizedProjects
            );
            busPublisher.Send(createNewUser, CorrelationContext.FromId(Guid.Parse(context.SagaId)));
        }

        public async Task HandleAsync(CreateUserRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            Data.RejectReason = message.Reason;
            await RejectAsync();
        }

        public async Task HandleAsync(ResourceCountIncremented message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            if (Data.NewRole != RolesEnum.User)
            {
                await SendNewUserEmail(context);
            }
            else
            {
                if (Data.NewAuthorizedProjects.Count() > 0)
                {
                    busPublisher.Send(new UpdateUserProjects(Data.Domain, Data.NewAuthorizedProjects, Data.UserId, Data.UserEmail, Data.Initiator), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                }
                else
                {
                    await SendNewUserEmail(context);
                }
            }
        }

        public async Task HandleAsync(IncrementResourceCountRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            await RejectAsync();
        }

        private async Task SendNewUserEmail(ISagaContext context)
        {
            IEnumerable<string> authorizedProjectNames = await Utilities.GetProjectNamesForAuthorizedProjectsAsync(Data.Domain, Data.UserEmail, Data.NewRole, Data.NewAuthorizedProjects, projectsClient).ConfigureAwait(false);

            var organizationNameModel = await tenantsClient.GetTenantAsync<TenantOrganizationNameModel>(Data.Domain).ConfigureAwait(false);
            await Task.Run(() =>
            {
                var sendNewUserEmail = new SendNewUserEmail(
                    Data.UserId,
                    Data.UserEmail,
                    Data.FirstName,
                    Data.Domain,
                    organizationNameModel.OrganizationName,
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
        public string RejectReason { get; set; }
        public string FirstName { get; set; }
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public string Token { get; set; }
        public RolesEnum NewRole { get; set; }
        public IEnumerable<Guid> NewAuthorizedProjects { get; set; }
    }
}
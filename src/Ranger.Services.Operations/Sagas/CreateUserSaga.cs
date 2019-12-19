using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chronicle;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;
using Ranger.Services.Operations.Messages.Notifications;
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
        private readonly IProjectsClient projectsClient;
        private readonly ITenantsClient tenantsClient;

        public CreateUserSaga(IBusPublisher busPublisher, IProjectsClient projectsClient, ITenantsClient tenantsClient, ILogger<CreateUserSaga> logger)
        {
            this.projectsClient = projectsClient;
            this.tenantsClient = tenantsClient;
            this.logger = logger;
            this.busPublisher = busPublisher;
        }

        public async Task CompensateAsync(UserCreated message, ISagaContext context)
        {
            await Task.Run(() => logger.LogError("Calling compensate for UserCreated."));
        }

        public async Task CompensateAsync(SendNewUserEmailSent message, ISagaContext context)
        {
            await Task.Run(() => logger.LogError("Calling compensate for SendNewUserEmailSent."));
        }

        public async Task CompensateAsync(CreateUserSagaInitializer message, ISagaContext context)
        {
            await Task.Run(() =>
            {
                logger.LogInformation("Calling compensate for CreateNewUserSagaInitializer.");
                busPublisher.Send(new SendPusherDomainUserCustomNotification(EVENT_NAME, $"Error creating user {Data.UserEmail}: {Data.RejectReason}", Data.Domain, Data.CommandingUserEmail, Operations.Data.OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            });
        }

        public async Task CompensateAsync(CreateUserRejected message, ISagaContext context)
        {
            await Task.Run(() =>
                logger.LogInformation("Calling compensate for CreateUserRejected.")
            );
        }

        public async Task CompensateAsync(UpdateUserProjectsRejected message, ISagaContext context)
        {
            await Task.Run(() =>
               logger.LogInformation("Calling compensate for UpdateUserProjectsRejected.")
            );
        }

        public async Task CompensateAsync(UserProjectsUpdated message, ISagaContext context)
        {
            await Task.Run(() =>
               logger.LogInformation("Calling compensate for UserProjectsUpdated.")
            );
        }

        public async Task HandleAsync(UpdateUserProjectsRejected message, ISagaContext context)
        {
            var notificationText = $"Successfully created {Data.UserEmail} but failed to set their authorized projects. Verify the selected projects and try again.";
            busPublisher.Send(new SendPusherDomainUserCustomNotification(EVENT_NAME, notificationText, Data.Domain, Data.CommandingUserEmail, Operations.Data.OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            await CompleteAsync();
        }

        public async Task HandleAsync(UserProjectsUpdated message, ISagaContext context)
        {
            var notificationText = "";
            if (message.UnSuccessfullyAddedProjectIds.Count() > 0)
            {
                notificationText = $"Successfully created {Data.UserEmail} but some projects failed be added. Verify the selected projects and try again.";
            }
            else
            {
                notificationText = $"User {Data.UserEmail} was successfully created.";
            }

            busPublisher.Send(new SendPusherDomainUserCustomNotification(EVENT_NAME, notificationText, Data.Domain, Data.CommandingUserEmail, Operations.Data.OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            await SendNewUserEmail(context);
            await CompleteAsync();
        }

        public async Task HandleAsync(UserCreated message, ISagaContext context)
        {
            Data.UserId = message.UserId;
            Data.FirstName = message.FirstName;
            Data.Token = message.Token;
            if (Data.NewRole != RolesEnum.User)
            {
                busPublisher.Send(new SendPusherDomainUserCustomNotification(EVENT_NAME, $"User {Data.UserEmail} was successfully created.", Data.Domain, Data.CommandingUserEmail, Operations.Data.OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                await SendNewUserEmail(context);
                await CompleteAsync();
            }
            else
            {
                if (Data.NewAuthorizedProjects.Count() > 0)
                {
                    busPublisher.Send(new UpdateUserProjects(Data.Domain, Data.NewAuthorizedProjects, message.UserId, message.Email, Data.CommandingUserEmail), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                }
                else
                {
                    busPublisher.Send(new SendPusherDomainUserCustomNotification(EVENT_NAME, $"User {Data.UserEmail} was successfully created.", Data.Domain, Data.CommandingUserEmail, Operations.Data.OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                    await SendNewUserEmail(context);
                    await CompleteAsync();
                }
            }
        }

        public async Task HandleAsync(SendNewUserEmailSent message, ISagaContext context)
        {
            busPublisher.Send(new SendPusherDomainUserCustomNotification(EVENT_NAME, $"User {Data.UserEmail} was succesfully created.", Data.Domain, Data.CommandingUserEmail, Operations.Data.OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            await CompleteAsync();
        }

        public async Task HandleAsync(CreateUserSagaInitializer message, ISagaContext context)
        {
            await Task.Run(() =>
            {
                Data.Domain = message.Domain;
                Data.UserEmail = message.Email;
                Data.CommandingUserEmail = message.CommandingUserEmail;
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
            });
        }

        public async Task HandleAsync(CreateUserRejected message, ISagaContext context)
        {
            Data.RejectReason = message.Reason;
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

    public class CreateUserData
    {
        public string RejectReason { get; set; }
        public string CommandingUserEmail { get; set; }
        public string FirstName { get; set; }
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public string Domain { get; set; }
        public string Token { get; set; }
        public RolesEnum NewRole { get; set; }
        public IEnumerable<string> NewAuthorizedProjects { get; set; }
    }
}
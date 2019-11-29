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

namespace Ranger.Services.Operations
{
    public class CreateUserSaga : Saga<CreateUserData>,
        ISagaStartAction<CreateUserSagaInitializer>,
        ISagaAction<UserCreated>,
        ISagaAction<CreateUserRejected>,
        ISagaAction<SendNewUserEmailSent>
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
            await Task.Run(() => logger.LogError("Calling compensate for NewUserCreated."));
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

        public async Task HandleAsync(UserCreated message, ISagaContext context)
        {
            var role = Enum.Parse<RolesEnum>(message.Role);
            IEnumerable<string> authorizedProjectNames = await Utilities.GetProjectNamesForAuthorizedProjectsAsync(message.Domain, role, message.AuthorizedProjects, projectsClient).ConfigureAwait(false);

            var organizationNameModel = await tenantsClient.GetTenantAsync<TenantOrganizationNameModel>(message.Domain).ConfigureAwait(false);
            await Task.Run(() =>
            {
                var sendNewUserEmail = new SendNewUserEmail(
                    message.UserId,
                    message.Email,
                    message.FirstName,
                    message.Domain,
                    organizationNameModel.OrganizationName,
                    message.Role,
                    message.Token,
                    authorizedProjectNames
                );
                this.busPublisher.Send(sendNewUserEmail, CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            });
        }


        public async Task HandleAsync(SendNewUserEmailSent message, ISagaContext context)
        {

            busPublisher.Send(new SendPusherDomainUserCustomNotification(EVENT_NAME, $"User {Data.UserEmail} succesfully created.", Data.Domain, Data.CommandingUserEmail, Operations.Data.OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            await CompleteAsync();
        }

        public async Task HandleAsync(CreateUserSagaInitializer message, ISagaContext context)
        {
            await Task.Run(() =>
            {
                Data.Domain = message.Domain;
                Data.UserEmail = message.Email;
                Data.CommandingUserEmail = message.CommandingUserEmail;

                var createNewUser = new CreateUser(
                    message.Domain,
                    message.Email,
                    message.FirstName,
                    message.LastName,
                    message.Role,
                    message.CommandingUserEmail,
                    message.PermittedProjectIds
                );
                busPublisher.Send(createNewUser, CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            });
        }

        public async Task HandleAsync(CreateUserRejected message, ISagaContext context)
        {
            Data.RejectReason = message.Reason;
            await RejectAsync();
        }
    }

    public class CreateUserData
    {
        public string RejectReason { get; set; }
        public string CommandingUserEmail { get; set; }
        public string UserEmail { get; set; }
        public string Domain { get; set; }
    }
}
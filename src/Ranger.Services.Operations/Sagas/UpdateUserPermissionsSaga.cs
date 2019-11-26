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

namespace Ranger.Services.Operations.Sagas
{
    public class UpdateUserPermissionsSaga : Saga<UpdateUserData>,
        ISagaStartAction<UpdateUserPermissionsSagaInitializer>,
        ISagaAction<UserPermissionsUpdated>,
        ISagaAction<UpdateUserPermissionsRejected>,
        ISagaAction<SendUserPermissionsUpdatedEmailSent>
    {
        const string EVENT_NAME = "user-created";
        private readonly IBusPublisher busPublisher;
        private readonly ILogger<UpdateUserPermissions> logger;
        private readonly IProjectsClient projectsClient;

        public UpdateUserPermissionsSaga(IBusPublisher busPublisher, IProjectsClient projectsClient, ILogger<UpdateUserPermissions> logger)
        {
            this.projectsClient = projectsClient;
            this.logger = logger;
            this.busPublisher = busPublisher;
        }

        public async Task CompensateAsync(UserPermissionsUpdated message, ISagaContext context)
        {
            await Task.Run(() => logger.LogError("Calling compensate for NewApplicationUserCreated."));
        }

        public async Task CompensateAsync(SendUserPermissionsUpdatedEmailSent message, ISagaContext context)
        {
            await Task.Run(() => logger.LogError("Calling compensate for SendNewUserEmailSent."));
        }

        public async Task CompensateAsync(UpdateUserPermissionsSagaInitializer message, ISagaContext context)
        {
            await Task.Run(() =>
            {
                logger.LogInformation("Calling compensate for CreateNewApplicationUserSagaInitializer.");
                busPublisher.Send(new SendPusherDomainUserCustomNotification(EVENT_NAME, $"Error creating user {Data.UserEmail}: {Data.RejectReason}", Data.Domain, Data.CommandingUserEmail, Operations.Data.OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            });
        }

        public async Task CompensateAsync(UpdateUserPermissionsRejected message, ISagaContext context)
        {
            await Task.Run(() =>
                logger.LogInformation("Calling compensate for UpdateUserPermissionsRejected.")
            );
        }

        public async Task HandleAsync(UserPermissionsUpdated message, ISagaContext context)
        {
            var projects = await projectsClient.GetAllProjectsAsync<IEnumerable<ProjectModel>>(message.Domain);
            IEnumerable<string> permittedProjectNames = null;
            if (message.Role.ToLowerInvariant() != Enum.GetName(typeof(RolesEnum), RolesEnum.User).ToLowerInvariant())
            {
                permittedProjectNames = projects.Where(_ => message.AuthorizedProjects.Contains(_.ProjectId)).Select(_ => _.Name);
            }
            else
            {
                permittedProjectNames = projects.Select(_ => _.Name);
            }

            await Task.Run(() =>
            {
                var sendNewUserEmail = new SendUserPermissionsUpdatedEmail(
                    message.UserId,
                    message.Email,
                    message.FirstName,
                    message.Domain,
                    message.Role,
                    permittedProjectNames
                );
                this.busPublisher.Send(sendNewUserEmail, CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            });
        }

        public async Task HandleAsync(SendUserPermissionsUpdatedEmailSent message, ISagaContext context)
        {

            busPublisher.Send(new SendPusherDomainUserCustomNotification(EVENT_NAME, $"User {Data.UserEmail} succesfully created.", Data.Domain, Data.CommandingUserEmail, Operations.Data.OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            await CompleteAsync();
        }

        public async Task HandleAsync(UpdateUserPermissionsSagaInitializer message, ISagaContext context)
        {
            await Task.Run(() =>
            {
                Data.Domain = message.Domain;
                Data.UserEmail = message.Email;
                Data.CommandingUserEmail = message.CommandingUserEmail;

                var createNewApplicationUser = new UpdateUserPermissions(
                    message.Domain,
                    message.Email,
                    message.CommandingUserEmail,
                    message.Role,
                    message.AuthorizedProjects
                    );
                busPublisher.Send(createNewApplicationUser, CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            });
        }

        public async Task HandleAsync(UpdateUserPermissionsRejected message, ISagaContext context)
        {
            Data.RejectReason = message.Reason;
            await RejectAsync();
        }
    }

    public class UpdateUserData
    {
        public string RejectReason { get; set; }
        public string CommandingUserEmail { get; set; }
        public string UserEmail { get; set; }
        public string Domain { get; set; }
    }
}
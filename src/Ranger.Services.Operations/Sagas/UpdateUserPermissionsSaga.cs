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

namespace Ranger.Services.Operations.Sagas
{
    public class UpdateUserPermissionsSaga : Saga<UpdateUserData>,
        ISagaStartAction<UpdateUserPermissionsSagaInitializer>,
        ISagaAction<UserRoleUpdated>,
        ISagaAction<UpdateUserRoleRejected>,
        ISagaAction<UserProjectsUpdated>,
        ISagaAction<UpdateUserProjectsRejected>
    {
        const string EVENT_NAME = "user-updated";
        private readonly IBusPublisher busPublisher;
        private readonly ILogger<UpdateUserPermissionsSaga> logger;
        private readonly IProjectsClient projectsClient;
        private readonly ITenantsClient tenantsClient;
        private readonly IIdentityClient identityClient;

        public UpdateUserPermissionsSaga(IBusPublisher busPublisher, IProjectsClient projectsClient, ITenantsClient tenantsClient, ILogger<UpdateUserPermissionsSaga> logger)
        {
            this.projectsClient = projectsClient;
            this.tenantsClient = tenantsClient;
            this.logger = logger;
            this.busPublisher = busPublisher;
        }

        public async Task CompensateAsync(UserRoleUpdated message, ISagaContext context)
        {
            await Task.Run(() => logger.LogError("Calling compensate for NewApplicationUserCreated."));
        }

        public async Task CompensateAsync(UpdateUserPermissionsSagaInitializer message, ISagaContext context)
        {
            await Task.Run(() =>
            {
                logger.LogInformation("Calling compensate for CreateNewApplicationUserSagaInitializer.");
                busPublisher.Send(new SendPusherDomainUserCustomNotification(EVENT_NAME, $"Error updating user {Data.UserEmail}: {Data.RejectReason}", Data.Domain, Data.CommandingUserEmail, Operations.Data.OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            });
        }

        public async Task CompensateAsync(UpdateUserRoleRejected message, ISagaContext context)
        {
            await Task.Run(() =>
               logger.LogInformation("Calling compensate for UpdateUserRoleRejected.")
            );
        }

        public async Task CompensateAsync(UserProjectsUpdated message, ISagaContext context)
        {
            await Task.Run(() =>
               logger.LogInformation("Calling compensate for UserProjectsUpdated.")
            );
        }

        public async Task CompensateAsync(UpdateUserProjectsRejected message, ISagaContext context)
        {
            await Task.Run(() =>
               logger.LogInformation("Calling compensate for UpdateUserProjectsRejected.")
            );
        }

        public async Task HandleAsync(UserRoleUpdated message, ISagaContext context)
        {
            Data.UserId = message.UserId;
            Data.FirstName = message.FirstName;
            if (Data.NewRole != RolesEnum.User)
            {
                busPublisher.Send(new SendPusherDomainUserCustomNotification(EVENT_NAME, $"Permissions successfully updated for {Data.UserEmail}.", Data.Domain, Data.CommandingUserEmail, Operations.Data.OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                await SendPermissionsUpdatedEmail(context);
                await CompleteAsync();
            }
            else
            {
                busPublisher.Send(new UpdateUserProjects(Data.Domain, Data.NewAuthorizedProjects, message.UserId, message.Email, Data.CommandingUserEmail), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            }
        }

        public async Task HandleAsync(UpdateUserPermissionsSagaInitializer message, ISagaContext context)
        {
            await Task.Run(() =>
            {
                Data.Domain = message.Domain;
                Data.UserEmail = message.Email;
                Data.CommandingUserEmail = message.CommandingUserEmail;
                Data.NewAuthorizedProjects = message.AuthorizedProjects;
                Data.NewRole = Enum.Parse<RolesEnum>(message.Role);

                var UpdateUserRole = new UpdateUserRole(
                    message.Domain,
                    message.Email,
                    message.CommandingUserEmail,
                    message.Role
                );
                busPublisher.Send(UpdateUserRole, CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            });
        }

        public async Task HandleAsync(UpdateUserRoleRejected message, ISagaContext context)
        {
            Data.RejectReason = message.Reason;
            await RejectAsync();
        }

        public async Task HandleAsync(UserProjectsUpdated message, ISagaContext context)
        {
            var notificationText = "";
            if (message.UnSuccessfullyAddedProjectIds.Count() > 0 && message.UnSuccessfullyRemovedProjectIds.Count() > 0)
            {
                notificationText = $"Permissions were successfully updated for {Data.UserEmail} but some projects failed be added and removed. Verify the selected projects and try again.";
            }
            else if (message.UnSuccessfullyAddedProjectIds.Count() > 0)
            {
                notificationText = $"Permissions were successfully updated for {Data.UserEmail} but some projects failed be added. Verify the selected projects and try again.";
            }
            else if (message.UnSuccessfullyRemovedProjectIds.Count() > 0)
            {
                notificationText = $"Permissions were successfully updated for {Data.UserEmail} but some projects failed be removed. Verify the selected projects and try again.";
            }
            else
            {
                notificationText = $"Permissions successfully updated for {Data.UserEmail}.";
            }

            busPublisher.Send(new SendPusherDomainUserCustomNotification(EVENT_NAME, notificationText, Data.Domain, Data.CommandingUserEmail, Operations.Data.OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            await SendPermissionsUpdatedEmail(context);
            await CompleteAsync();
        }

        public async Task HandleAsync(UpdateUserProjectsRejected message, ISagaContext context)
        {
            var notificationText = "The user was added to their new role but failed to set their authorized projects. Verify the selected projects and try again.";
            busPublisher.Send(new SendPusherDomainUserCustomNotification(EVENT_NAME, notificationText, Data.Domain, Data.CommandingUserEmail, Operations.Data.OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            await CompleteAsync();
        }

        private async Task SendPermissionsUpdatedEmail(ISagaContext context)
        {
            try
            {
                IEnumerable<string> authorizedProjectNames = await Utilities.GetProjectNamesForAuthorizedProjectsAsync(Data.Domain, Data.UserEmail, Data.NewRole, Data.NewAuthorizedProjects, projectsClient).ConfigureAwait(false);

                var organizationNameModel = await tenantsClient.GetTenantAsync<TenantOrganizationNameModel>(Data.Domain).ConfigureAwait(false);

                await Task.Run(() =>
                {
                    var sendNewUserEmail = new SendUserPermissionsUpdatedEmail(
                        Data.UserId,
                        Data.UserEmail,
                        Data.FirstName,
                        Data.Domain,
                        organizationNameModel.OrganizationName,
                        Enum.GetName(typeof(RolesEnum), Data.NewRole),
                        authorizedProjectNames
                    );
                    this.busPublisher.Send(sendNewUserEmail, CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to gather necessay requirements to send updated permissions email. Permissions were updated successfully, silently failing and completing saga.");
            }
        }
    }

    public class UpdateUserData
    {
        public string RejectReason { get; set; }
        public string CommandingUserEmail { get; set; }
        public string FirstName { get; set; }
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public string Domain { get; set; }
        public RolesEnum NewRole { get; set; }
        public IEnumerable<string> NewAuthorizedProjects { get; set; }
    }
}
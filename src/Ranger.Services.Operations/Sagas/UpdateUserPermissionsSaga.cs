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
        const string EVENT_NAME = "user-updated";
        private readonly IBusPublisher busPublisher;
        private readonly ILogger<UpdateUserPermissions> logger;
        private readonly IProjectsClient projectsClient;
        private readonly ITenantsClient tenantsClient;

        public UpdateUserPermissionsSaga(IBusPublisher busPublisher, IProjectsClient projectsClient, ITenantsClient tenantsClient, ILogger<UpdateUserPermissions> logger)
        {
            this.projectsClient = projectsClient;
            this.tenantsClient = tenantsClient;
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
                busPublisher.Send(new SendPusherDomainUserCustomNotification(EVENT_NAME, $"Error updating user {Data.UserEmail}: {Data.RejectReason}", Data.Domain, Data.CommandingUserEmail, Operations.Data.OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
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
            try
            {
                var role = Enum.Parse<RolesEnum>(message.Role);
                IEnumerable<string> authorizedProjectNames = await Utilities.GetProjectNamesForAuthorizedProjectsAsync(message.Domain, role, message.AuthorizedProjects, projectsClient).ConfigureAwait(false);

                var organizationNameModel = await tenantsClient.GetTenantAsync<TenantOrganizationNameModel>(message.Domain).ConfigureAwait(false);
                await Task.Run(() =>
                    {
                        var sendNewUserEmail = new SendUserPermissionsUpdatedEmail(
                            message.UserId,
                            message.Email,
                            message.FirstName,
                            message.Domain,
                            organizationNameModel.OrganizationName,
                            message.Role,
                            authorizedProjectNames
                        );
                        this.busPublisher.Send(sendNewUserEmail, CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                    });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to gather necessay requirements to send updated permissions email. Permissions were updated successfully, silently failing and completing saga.");
                await CompleteAsync();
            }
        }

        public async Task HandleAsync(SendUserPermissionsUpdatedEmailSent message, ISagaContext context)
        {
            busPublisher.Send(new SendPusherDomainUserCustomNotification(EVENT_NAME, $"Permissions successfully updated for {Data.UserEmail}.", Data.Domain, Data.CommandingUserEmail, Operations.Data.OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            await CompleteAsync();
        }

        public async Task HandleAsync(UpdateUserPermissionsSagaInitializer message, ISagaContext context)
        {
            await Task.Run(() =>
            {
                Data.Domain = message.Domain;
                Data.UserEmail = message.Email;
                Data.CommandingUserEmail = message.CommandingUserEmail;

                var updateUserPermissions = new UpdateUserPermissions(
                    message.Domain,
                    message.Email,
                    message.CommandingUserEmail,
                    message.Role,
                    message.AuthorizedProjects
                    );
                busPublisher.Send(updateUserPermissions, CorrelationContext.FromId(Guid.Parse(context.SagaId)));
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
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
using Ranger.Services.Operations.Messages.Notifications;
using Ranger.Services.Operations.Messages.Operations;
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
        private readonly IProjectsHttpClient projectsClient;

        public UpdateUserPermissionsSaga(IBusPublisher busPublisher, IProjectsHttpClient projectsClient, ILogger<UpdateUserPermissionsSaga> logger)
        {
            this.projectsClient = projectsClient;
            this.logger = logger;
            this.busPublisher = busPublisher;
        }

        public Task CompensateAsync(UserRoleUpdated message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public async Task CompensateAsync(UpdateUserPermissionsSagaInitializer message, ISagaContext context)
        {
            await Task.Run(() =>
            {
                logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
                busPublisher.Send(new SendPusherDomainUserCustomNotification(EVENT_NAME, $"Error updating user {Data.UserEmail}: {Data.RejectReason}", Data.TenantId, Data.Initiator, Operations.Data.OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            });
        }

        public Task CompensateAsync(UpdateUserRoleRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(UserProjectsUpdated message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(UpdateUserProjectsRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public async Task HandleAsync(UserRoleUpdated message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            Data.UserId = message.UserId;
            Data.FirstName = message.FirstName;
            if (Data.NewRole != RolesEnum.User)
            {
                busPublisher.Send(new SendPusherDomainUserCustomNotification(EVENT_NAME, $"Permissions successfully updated for {Data.UserEmail}", Data.TenantId, Data.Initiator, Operations.Data.OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                await SendPermissionsUpdatedEmail(context);
                await SendPermissionsUpdatedPusherNotification(context);
                await CompleteAsync();
            }
            else
            {
                busPublisher.Send(new UpdateUserProjects(Data.TenantId, Data.NewAuthorizedProjects, message.UserId, message.Email, Data.Initiator), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            }
        }

        public Task HandleAsync(UpdateUserPermissionsSagaInitializer message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            Data.TenantId = message.TenantId;
            Data.UserEmail = message.Email;
            Data.Initiator = message.CommandingUserEmail;
            Data.NewAuthorizedProjects = message.AuthorizedProjects;
            Data.NewRole = message.Role;

            var UpdateUserRole = new UpdateUserRole(
                message.TenantId,
                message.Email,
                message.CommandingUserEmail,
                message.Role
            );
            busPublisher.Send(UpdateUserRole, CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public async Task HandleAsync(UpdateUserRoleRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            Data.RejectReason = message.Reason;
            await RejectAsync();
        }

        public async Task HandleAsync(UserProjectsUpdated message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            var notificationText = "";
            if (message.UnSuccessfullyAddedProjectIds.Count() > 0 && message.UnSuccessfullyRemovedProjectIds.Count() > 0)
            {
                notificationText = $"Permissions were successfully updated for {Data.UserEmail} but some projects failed be added and removed. Verify the selected projects and try again";
            }
            else if (message.UnSuccessfullyAddedProjectIds.Count() > 0)
            {
                notificationText = $"Permissions were successfully updated for {Data.UserEmail} but some projects failed be added. Verify the selected projects and try again";
            }
            else if (message.UnSuccessfullyRemovedProjectIds.Count() > 0)
            {
                notificationText = $"Permissions were successfully updated for {Data.UserEmail} but some projects failed be removed. Verify the selected projects and try again";
            }
            else
            {
                notificationText = $"Permissions successfully updated for {Data.UserEmail}";
            }

            busPublisher.Send(new SendPusherDomainUserCustomNotification(EVENT_NAME, notificationText, Data.TenantId, Data.Initiator, Operations.Data.OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            await SendPermissionsUpdatedEmail(context);
            await SendPermissionsUpdatedPusherNotification(context);
            await CompleteAsync();
        }

        public async Task HandleAsync(UpdateUserProjectsRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            var notificationText = "The user was added to their new role but failed to set their authorized projects. Verify the selected projects and try again";
            busPublisher.Send(new SendPusherDomainUserCustomNotification(EVENT_NAME, notificationText, Data.TenantId, Data.Initiator, Operations.Data.OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            await SendPermissionsUpdatedPusherNotification(context);
            await CompleteAsync();
        }

        private async Task SendPermissionsUpdatedPusherNotification(ISagaContext context)
        {
            await Task.Run(() => busPublisher.Send(new SendPusherDomainUserPredefinedNotification("PermissionsUpdated", Data.TenantId, Data.UserEmail), CorrelationContext.FromId(Guid.Parse(context.SagaId))));
        }

        private async Task SendPermissionsUpdatedEmail(ISagaContext context)
        {
            try
            {
                IEnumerable<string> authorizedProjectNames = await Utilities.GetProjectNamesForAuthorizedProjectsAsync(Data.TenantId, Data.UserEmail, Data.NewRole, Data.NewAuthorizedProjects, projectsClient).ConfigureAwait(false);

                await Task.Run(() =>
                {
                    var sendNewUserEmail = new SendUserPermissionsUpdatedEmail(
                        Data.UserId,
                        Data.UserEmail,
                        Data.FirstName,
                        Data.TenantId,
                        Enum.GetName(typeof(RolesEnum), Data.NewRole),
                        authorizedProjectNames
                    );
                    this.busPublisher.Send(sendNewUserEmail, CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to gather necessay requirements to send updated permissions email. Permissions were updated successfully, silently failing and completing saga");
            }
        }
    }

    public class UpdateUserData : BaseSagaData
    {
        public string RejectReason { get; set; }
        public string FirstName { get; set; }
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public RolesEnum NewRole { get; set; }
        public IEnumerable<Guid> NewAuthorizedProjects { get; set; }
    }
}
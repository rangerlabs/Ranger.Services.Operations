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
    public class NewApplicationUser : Saga<UserData>,
        ISagaStartAction<CreateNewApplicationUserSagaInitializer>,
        ISagaAction<NewApplicationUserCreated>,
        ISagaAction<CreateApplicationUserRejected>,
        ISagaAction<SendNewUserEmailSent>
    {
        const string EVENT_NAME = "user-created";
        private readonly IBusPublisher busPublisher;
        private readonly ILogger<NewApplicationUser> logger;
        private readonly IProjectsClient projectsClient;

        public NewApplicationUser(IBusPublisher busPublisher, IProjectsClient projectsClient, ILogger<NewApplicationUser> logger)
        {
            this.projectsClient = projectsClient;
            this.logger = logger;
            this.busPublisher = busPublisher;
        }

        public async Task CompensateAsync(NewApplicationUserCreated message, ISagaContext context)
        {
            await Task.Run(() => logger.LogError("Calling compensate for NewApplicationUserCreated."));
        }

        public async Task CompensateAsync(SendNewUserEmailSent message, ISagaContext context)
        {
            await Task.Run(() => logger.LogError("Calling compensate for SendNewUserEmailSent."));
        }

        public async Task CompensateAsync(CreateNewApplicationUserSagaInitializer message, ISagaContext context)
        {
            await Task.Run(() =>
            {
                logger.LogInformation("Calling compensate for CreateNewApplicationUserSagaInitializer.");
                busPublisher.Send(new SendPusherDomainUserCustomNotification(EVENT_NAME, $"Error creating user {Data.UserEmail}: {Data.RejectReason}", Data.Domain, Data.CommandingUserEmail, Operations.Data.OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            });
        }

        public async Task CompensateAsync(CreateApplicationUserRejected message, ISagaContext context)
        {
            await Task.Run(() =>
                logger.LogInformation("Calling compensate for CreateApplicationUserRejected.")
            );
        }

        public async Task HandleAsync(NewApplicationUserCreated message, ISagaContext context)
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
                var sendNewUserEmail = new SendNewUserEmail(
                    message.Email,
                    message.FirstName,
                    message.Domain,
                    message.Role,
                    message.RegistrationKey,
                    permittedProjectNames
                );
                this.busPublisher.Send(sendNewUserEmail, CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            });
        }

        public async Task HandleAsync(SendNewUserEmailSent message, ISagaContext context)
        {

            busPublisher.Send(new SendPusherDomainUserCustomNotification(EVENT_NAME, $"User {Data.UserEmail} created.", Data.Domain, Data.CommandingUserEmail, Operations.Data.OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            await CompleteAsync();
        }

        public async Task HandleAsync(CreateNewApplicationUserSagaInitializer message, ISagaContext context)
        {
            await Task.Run(() =>
            {
                Data.Domain = message.Domain;
                Data.UserEmail = message.Email;
                Data.CommandingUserEmail = message.CommandingUserEmail;

                var createNewApplicationUser = new CreateApplicationUser(
                    message.Domain,
                    message.Email,
                    message.FirstName,
                    message.LastName,
                    message.Role,
                    message.CommandingUserEmail,
                    message.PermittedProjectIds
                );
                busPublisher.Send(createNewApplicationUser, CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            });
        }

        public async Task HandleAsync(CreateApplicationUserRejected message, ISagaContext context)
        {
            Data.RejectReason = message.Reason;
            await RejectAsync();
        }
    }

    public class UserData
    {
        public string RejectReason { get; set; }
        public string CommandingUserEmail { get; set; }
        public string UserEmail { get; set; }
        public string Domain { get; set; }
    }
}
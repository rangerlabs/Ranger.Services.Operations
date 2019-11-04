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
    public class NewUser : Saga<UserData>,
        ISagaStartAction<NewApplicationUserCreated>,
        ISagaAction<SendNewUserEmailSent>
    {
        private readonly IBusPublisher busPublisher;
        private readonly ILogger<NewUser> logger;
        private readonly IProjectsClient projectsClient;

        public NewUser(IBusPublisher busPublisher, IProjectsClient projectsClient, ILogger<NewUser> logger)
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

        public async Task HandleAsync(NewApplicationUserCreated message, ISagaContext context)
        {
            Data.Domain = message.Domain;
            Data.UserEmail = message.CommandingUserEmail;

            var projects = await projectsClient.GetAllProjectsAsync<IEnumerable<ProjectModel>>(message.Domain);
            IEnumerable<string> permittedProjectNames = null;
            if (message.Role.ToLowerInvariant() != Enum.GetName(typeof(RolesEnum), RolesEnum.User).ToLowerInvariant())
            {
                permittedProjectNames = projects.Where(_ => message.PermittedProjects.Contains(_.ProjectId)).Select(_ => _.Name);
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
            await Task.Run(() =>
            {
                busPublisher.Send(new SendPusherPrivateFrontendNotification("NewUser", Data.Domain, Data.UserEmail, Operations.Data.OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            });
        }
    }

    public class UserData
    {
        public string UserEmail { get; set; }
        public string Domain { get; set; }
    }
}
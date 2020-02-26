using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chronicle;
using Microsoft.Extensions.Logging;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;
using Ranger.Services.Operations.Data;
using Ranger.Services.Operations.Messages.Identity;
using Ranger.Services.Operations.Messages.Notifications;
using Ranger.Services.Operations.Messages.Tenants;
using Ranger.Services.Operations.Messages.Tenants.Commands;

namespace Ranger.Services.Operations
{
    //This is the one time not to use a SagaInitializer because DatabaseUsername has not yet been assigned 
    //and a SagaState cannot be persisted as a result of the database constraint
    public class TenantOnboardingSaga : BaseSaga<TenantOnboardingSaga, UserData>,
        ISagaStartAction<Messages.Tenants.TenantCreated>,
        ISagaAction<Messages.Identity.TenantInitialized>,
        ISagaAction<Messages.Projects.TenantInitialized>,
        ISagaAction<Messages.Integrations.TenantInitialized>,
        ISagaAction<Messages.Identity.InitializeTenantRejected>,
        ISagaAction<Messages.Projects.InitializeTenantRejected>,
        ISagaAction<Messages.Integrations.InitializeTenantRejected>,
        ISagaAction<NewPrimaryOwnerCreated>,
        ISagaAction<SendNewPrimaryOwnerEmailSent>
    {
        private readonly IBusPublisher busPublisher;
        private readonly ILogger<TenantOnboardingSaga> logger;
        private readonly ITenantsClient tenantsClient;

        public TenantOnboardingSaga(IBusPublisher busPublisher, ITenantsClient tenantsClient, ILogger<TenantOnboardingSaga> logger) : base(tenantsClient, logger)
        {
            this.tenantsClient = tenantsClient;
            this.busPublisher = busPublisher;
            this.logger = logger;
        }

        public async Task HandleAsync(TenantCreated message, ISagaContext context)
        {
            await Task.Run(() =>
            {
                Data.Token = message.Token;
                Data.Domain = message.Domain;
                Data.Initiator = message.Email;
                Data.FirstName = message.FirstName;
                Data.LastName = message.LastName;
                Data.Password = message.Password;
                Data.OrganizationName = message.OrganizationName;
                Data.DatabaseUsername = message.DatabaseUsername;
                Data.DatabasePassword = message.DatabasePassword;
                this.busPublisher.Send(new Messages.Identity.InitializeTenant(message.DatabaseUsername, message.DatabasePassword), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            });
        }

        public async Task CompensateAsync(TenantCreated message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            logger.LogDebug("Dropping tenant.");
            await Task.Run(() =>
            {
                this.busPublisher.Send(new Messages.Identity.DropTenant(Data.DatabaseUsername), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                this.busPublisher.Send(new Messages.Projects.DropTenant(Data.DatabaseUsername), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                this.busPublisher.Send(new Messages.Integrations.DropTenant(Data.DatabaseUsername), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            });
            await Task.CompletedTask;
            await Task.Run(() =>
               this.busPublisher.Send(
                   new DeleteTenant("System", Data.Domain),
                   CorrelationContext.FromId(Guid.Parse(context.SagaId))
               )
            );
        }

        public async Task HandleAsync(Messages.Identity.TenantInitialized message, ISagaContext context)
        {
            await Task.Run(() =>
               this.busPublisher.Send(new Messages.Projects.InitializeTenant(Data.DatabaseUsername, Data.DatabasePassword), CorrelationContext.FromId(Guid.Parse(context.SagaId)))
            );
        }

        public async Task CompensateAsync(Messages.Identity.TenantInitialized message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            await Task.CompletedTask;
        }

        public async Task HandleAsync(Messages.Identity.InitializeTenantRejected message, ISagaContext context)
        {
            await RejectAsync();
        }

        public async Task CompensateAsync(Messages.Identity.InitializeTenantRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            await Task.CompletedTask;
        }

        public async Task HandleAsync(NewPrimaryOwnerCreated message, ISagaContext context)
        {
            await Task.Run(() =>
            {
                busPublisher.Send(new SendNewPrimaryOwnerEmail(Data.Initiator, Data.FirstName, Data.Domain, Data.Token), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            });
        }

        public async Task CompensateAsync(NewPrimaryOwnerCreated message, ISagaContext context)
        {
            await Task.CompletedTask;
        }

        public async Task HandleAsync(SendNewPrimaryOwnerEmailSent message, ISagaContext context)
        {
            busPublisher.Send<SendPusherDomainFrontendNotification>(
                new SendPusherDomainFrontendNotification(
                    "TenantOnboarding",
                    Data.Domain,
                    OperationsStateEnum.Completed),
                CorrelationContext.FromId(Guid.Parse(context.SagaId)));

            logger.LogInformation("TenantUserSignup saga completed succesfully.");
            await CompleteAsync();
        }

        public async Task CompensateAsync(SendNewPrimaryOwnerEmailSent message, ISagaContext context)
        {
            await Task.CompletedTask;
        }

        public async Task HandleAsync(Messages.Projects.InitializeTenantRejected message, ISagaContext context)
        {
            await RejectAsync();
        }

        public async Task CompensateAsync(Messages.Projects.InitializeTenantRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            await Task.CompletedTask;
        }

        public async Task HandleAsync(Messages.Integrations.InitializeTenantRejected message, ISagaContext context)
        {
            await RejectAsync();
        }

        public async Task CompensateAsync(Messages.Integrations.InitializeTenantRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            await Task.CompletedTask;
        }

        public async Task HandleAsync(Messages.Projects.TenantInitialized message, ISagaContext context)
        {
            await Task.Run(() =>
            {
                this.busPublisher.Send(new Messages.Integrations.InitializeTenant(Data.DatabaseUsername, Data.DatabasePassword), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            });
        }

        public async Task CompensateAsync(Messages.Projects.TenantInitialized message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            await Task.CompletedTask;
        }

        public async Task HandleAsync(Messages.Integrations.TenantInitialized message, ISagaContext context)
        {
            await Task.Run(() =>
            {
                this.busPublisher.Send(
                    new CreateNewPrimaryOwner(
                        Data.Initiator,
                        Data.FirstName,
                        Data.LastName,
                        Data.Password,
                        Data.Domain
                    ),
                    CorrelationContext.FromId(Guid.Parse(context.SagaId))
                );
            }); ;
        }

        public async Task CompensateAsync(Messages.Integrations.TenantInitialized message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            await Task.CompletedTask;
        }
    }

    public class UserData : BaseSagaData
    {
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Token { get; set; }
        public string DatabasePassword { get; set; }
        public string OrganizationName { get; set; }
        public List<string> ServicesInitialized { get; set; } = new List<string>();
    }
}
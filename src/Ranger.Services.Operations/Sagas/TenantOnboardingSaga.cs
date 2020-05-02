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
using Ranger.Services.Operations.Messages.Subscriptions;
using Ranger.Services.Operations.Messages.Tenants;
using Ranger.Services.Operations.Messages.Tenants.Commands;

namespace Ranger.Services.Operations
{
    //This is the one time not to use a SagaInitializer because TenantId has not yet been assigned 
    //and a SagaState cannot be persisted as a result of the database constraint
    public class TenantOnboardingSaga : Saga<UserData>,
        ISagaStartAction<Messages.Tenants.TenantCreated>,
        ISagaAction<Messages.Identity.TenantInitialized>,
        ISagaAction<Messages.Projects.TenantInitialized>,
        ISagaAction<Messages.Integrations.TenantInitialized>,
        ISagaAction<Messages.Breadcrumbs.TenantInitialized>,
        ISagaAction<Messages.Identity.InitializeTenantRejected>,
        ISagaAction<Messages.Projects.InitializeTenantRejected>,
        ISagaAction<Messages.Integrations.InitializeTenantRejected>,
        ISagaAction<Messages.Breadcrumbs.InitializeTenantRejected>,
        ISagaAction<NewPrimaryOwnerCreated>,
        ISagaAction<Messages.Subscriptions.NewTenantSubscriptionCreated>,
        ISagaAction<NewTenantSubscriptionRejected>,
        ISagaAction<SendNewPrimaryOwnerEmailSent>
    {
        private readonly IBusPublisher busPublisher;
        private readonly ILogger<TenantOnboardingSaga> logger;

        public TenantOnboardingSaga(IBusPublisher busPublisher, ILogger<TenantOnboardingSaga> logger)
        {
            this.busPublisher = busPublisher;
            this.logger = logger;
        }

        public Task HandleAsync(TenantCreated message, ISagaContext context)
        {
            Data.Token = message.Token;
            Data.TenantId = message.TenantId;
            Data.Initiator = message.Email;
            Data.FirstName = message.FirstName;
            Data.LastName = message.LastName;
            Data.Password = message.Password;
            Data.OrganizationName = message.OrganizationName;
            Data.TenantId = message.TenantId;
            Data.DatabasePassword = message.DatabasePassword;
            this.busPublisher.Send(new Messages.Identity.InitializeTenant((string)message.TenantId, message.DatabasePassword), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public Task CompensateAsync(TenantCreated message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            logger.LogDebug("Dropping tenant");
            this.busPublisher.Send(new Messages.Identity.DropTenant(Data.TenantId), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            this.busPublisher.Send(new Messages.Projects.DropTenant(Data.TenantId), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            this.busPublisher.Send(new Messages.Breadcrumbs.DropTenant(Data.TenantId), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            this.busPublisher.Send(new Messages.Integrations.DropTenant(Data.TenantId), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            this.busPublisher.Send(new DeleteTenant("Operations", Data.TenantId), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public Task HandleAsync(Messages.Identity.TenantInitialized message, ISagaContext context)
        {
            this.busPublisher.Send(new Messages.Projects.InitializeTenant(Data.TenantId, Data.DatabasePassword), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public Task CompensateAsync(Messages.Identity.TenantInitialized message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public async Task HandleAsync(Messages.Identity.InitializeTenantRejected message, ISagaContext context)
        {
            await RejectAsync();
        }

        public Task CompensateAsync(Messages.Identity.InitializeTenantRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public async Task HandleAsync(NewPrimaryOwnerCreated message, ISagaContext context)
        {
            await Task.Run(() =>
            {
                busPublisher.Send(new CreateNewTenantSubscription(Data.TenantId, Data.Initiator, Data.FirstName, Data.LastName, Data.OrganizationName), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            });
        }

        public Task CompensateAsync(NewPrimaryOwnerCreated message, ISagaContext context)
        {
            return Task.CompletedTask;
        }

        public Task HandleAsync(NewTenantSubscriptionCreated message, ISagaContext context)
        {
            busPublisher.Send(new SendNewPrimaryOwnerEmail(Data.Initiator, Data.FirstName, Data.TenantId, Data.Token), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public Task CompensateAsync(NewTenantSubscriptionCreated message, ISagaContext context)
        {
            return Task.CompletedTask;
        }

        public async Task HandleAsync(SendNewPrimaryOwnerEmailSent message, ISagaContext context)
        {
            busPublisher.Send<SendPusherDomainFrontendNotification>(
                new SendPusherDomainFrontendNotification(
                    "TenantOnboarding",
                    Data.TenantId,
                    OperationsStateEnum.Completed),
                CorrelationContext.FromId(Guid.Parse(context.SagaId)));

            logger.LogInformation("TenantUserSignup saga completed succesfully");
            await CompleteAsync();
        }

        public Task CompensateAsync(SendNewPrimaryOwnerEmailSent message, ISagaContext context)
        {
            return Task.CompletedTask;
        }

        public async Task HandleAsync(Messages.Projects.InitializeTenantRejected message, ISagaContext context)
        {
            await RejectAsync();
        }

        public Task CompensateAsync(Messages.Projects.InitializeTenantRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public async Task HandleAsync(Messages.Breadcrumbs.InitializeTenantRejected message, ISagaContext context)
        {
            await RejectAsync();
        }

        public Task CompensateAsync(Messages.Breadcrumbs.InitializeTenantRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public async Task HandleAsync(Messages.Integrations.InitializeTenantRejected message, ISagaContext context)
        {
            await RejectAsync();
        }

        public Task CompensateAsync(Messages.Integrations.InitializeTenantRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task HandleAsync(Messages.Projects.TenantInitialized message, ISagaContext context)
        {
            this.busPublisher.Send(new Messages.Breadcrumbs.InitializeTenant(Data.TenantId, Data.DatabasePassword), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public Task CompensateAsync(Messages.Projects.TenantInitialized message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task HandleAsync(Messages.Breadcrumbs.TenantInitialized message, ISagaContext context)
        {
            this.busPublisher.Send(new Messages.Integrations.InitializeTenant(Data.TenantId, Data.DatabasePassword), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public Task CompensateAsync(Messages.Breadcrumbs.TenantInitialized message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task HandleAsync(Messages.Integrations.TenantInitialized message, ISagaContext context)
        {
            this.busPublisher.Send(
                new CreateNewPrimaryOwner(
                    Data.Initiator,
                    Data.FirstName,
                    Data.LastName,
                    Data.Password,
                    Data.TenantId
                ),
                CorrelationContext.FromId(Guid.Parse(context.SagaId))
            );
            return Task.CompletedTask;
        }

        public Task CompensateAsync(Messages.Integrations.TenantInitialized message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public async Task HandleAsync(NewTenantSubscriptionRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            await RejectAsync();
        }

        public Task CompensateAsync(NewTenantSubscriptionRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
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
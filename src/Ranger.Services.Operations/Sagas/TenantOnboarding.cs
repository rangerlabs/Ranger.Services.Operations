using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chronicle;
using Microsoft.Extensions.Logging;
using Ranger.RabbitMQ;
using Ranger.Services.Operations.Data;
using Ranger.Services.Operations.Messages.Identity;
using Ranger.Services.Operations.Messages.Notifications;
using Ranger.Services.Operations.Messages.Tenants;

namespace Ranger.Services.Operations
{
    public class TenantOnboarding : Saga<UserData>,
        ISagaStartAction<TenantCreated>,
        ISagaAction<Messages.Identity.TenantInitialized>,
        ISagaAction<Messages.Geofences.TenantInitialized>,
        ISagaAction<Messages.Projects.TenantInitialized>,
        ISagaAction<Messages.Integrations.TenantInitialized>,
        ISagaAction<Messages.Identity.InitializeTenantRejected>,
        ISagaAction<Messages.Geofences.InitializeTenantRejected>,
        ISagaAction<Messages.Projects.InitializeTenantRejected>,
        ISagaAction<Messages.Integrations.InitializeTenantRejected>,
        ISagaAction<NewTenantOwnerCreated>,
        ISagaAction<SendNewTenantOwnerEmailSent>
    {
        const int SERVICES_TO_BE_INITIALIZED = 4;
        private readonly IBusPublisher busPublisher;
        private readonly ILogger<TenantOnboarding> logger;

        public TenantOnboarding(IBusPublisher busPublisher, ILogger<TenantOnboarding> logger)
        {
            this.busPublisher = busPublisher;
            this.logger = logger;
        }

        public async Task HandleAsync(TenantCreated message, ISagaContext context)
        {
            await Task.Run(() =>
            {
                Data.Owner = message.Owner;
                Data.Domain = message.DomainName;
                Data.RegistrationKey = message.RegistrationKey;
                Data.DatabaseUsername = message.DatabaseUsername;
                this.busPublisher.Send(new Messages.Identity.InitializeTenant(message.DatabaseUsername, message.DatabasePassword), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                this.busPublisher.Send(new Messages.Projects.InitializeTenant(message.DatabaseUsername, message.DatabasePassword), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                this.busPublisher.Send(new Messages.Integrations.InitializeTenant(message.DatabaseUsername, message.DatabasePassword), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                this.busPublisher.Send(new Messages.Geofences.InitializeTenant(message.DatabaseUsername, message.DatabasePassword), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            });
        }

        public async Task CompensateAsync(TenantCreated message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            await DropTenant(context);
            await Task.Run(() =>
                this.busPublisher.Send(
                    new DeleteTenant(Data.Domain),
                    CorrelationContext.FromId(Guid.Parse(context.SagaId))
                )
            );
        }

        public async Task HandleAsync(Messages.Identity.TenantInitialized message, ISagaContext context)
        {
            await Task.Run(() =>
               this.busPublisher.Send(
                   new CreateNewTenantOwner(
                       Data.Owner.Email,
                       Data.Owner.FirstName,
                       Data.Owner.LastName,
                       Data.Owner.Password,
                       Data.Domain,
                       ""
                   ),
                   CorrelationContext.FromId(Guid.Parse(context.SagaId))
               )
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

        public async Task HandleAsync(NewTenantOwnerCreated message, ISagaContext context)
        {
            await Task.Run(() =>
            {
                if (AllServicesInitialized("identity"))
                {
                    SendNewTenantOwnerEmail(context);
                }
            });
        }

        public async Task CompensateAsync(NewTenantOwnerCreated message, ISagaContext context)
        {
            await Task.CompletedTask;
        }

        public async Task HandleAsync(SendNewTenantOwnerEmailSent message, ISagaContext context)
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

        public async Task CompensateAsync(SendNewTenantOwnerEmailSent message, ISagaContext context)
        {
            await Task.CompletedTask;
        }

        public async Task HandleAsync(Messages.Geofences.InitializeTenantRejected message, ISagaContext context)
        {
            await RejectAsync();
        }

        public async Task CompensateAsync(Messages.Geofences.InitializeTenantRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
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

        public async Task HandleAsync(Messages.Geofences.TenantInitialized message, ISagaContext context)
        {
            await Task.Run(() =>
            {
                if (AllServicesInitialized("geofences"))
                {
                    SendNewTenantOwnerEmail(context);
                }
            });
        }

        public async Task CompensateAsync(Messages.Geofences.TenantInitialized message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            await Task.CompletedTask;
        }

        public async Task HandleAsync(Messages.Projects.TenantInitialized message, ISagaContext context)
        {
            await Task.Run(() =>
            {
                if (AllServicesInitialized("projects"))
                {
                    SendNewTenantOwnerEmail(context);
                }
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
                if (AllServicesInitialized("integrations"))
                {
                    SendNewTenantOwnerEmail(context);
                }
            });
        }

        public async Task CompensateAsync(Messages.Integrations.TenantInitialized message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            await Task.CompletedTask;
        }

        private bool AllServicesInitialized(string service)
        {
            if (Data.ServicesInitialized.Count == SERVICES_TO_BE_INITIALIZED - 1)
            {
                return true;
            }
            Data.ServicesInitialized.Add(service);
            return false;
        }

        private async Task DropTenant(ISagaContext context)
        {
            logger.LogDebug("Dropping tenant.");
            await Task.Run(() =>
            {
                this.busPublisher.Send(new Messages.Identity.DropTenant(Data.DatabaseUsername), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                this.busPublisher.Send(new Messages.Geofences.DropTenant(Data.DatabaseUsername), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            });
            await Task.CompletedTask;
        }

        private void SendNewTenantOwnerEmail(ISagaContext context)
        {
            busPublisher.Send(new SendNewTenantOwnerEmail(Data.Owner.Email, Data.Owner.FirstName, Data.Domain, Data.RegistrationKey),
                CorrelationContext.FromId(Guid.Parse(context.SagaId))
            );
        }
    }

    public class UserData
    {
        public NewTenantOwner Owner { get; set; }
        public string Domain { get; set; }
        public string RegistrationKey { get; set; }
        public string DatabaseUsername { get; set; }
        public List<string> ServicesInitialized { get; set; } = new List<string>();
    }
}
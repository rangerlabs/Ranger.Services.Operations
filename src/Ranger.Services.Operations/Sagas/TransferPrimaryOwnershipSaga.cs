using System;
using System.Threading.Tasks;
using Chronicle;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;
using Ranger.Services.Operations.Data;
using Ranger.Services.Operations.Messages.Identity.Commands;
using Ranger.Services.Operations.Messages.Notifications;
using Ranger.Services.Operations.Messages.Operations.Commands;
using Ranger.Services.Operations.Messages.Tenants.Commands;
using Ranger.Services.Operations.Messages.Tenants.Events;
using Ranger.Services.Operations.Messages.Tenants.RejectedEvents;

namespace Ranger.Services.Operations.Sagas
{
    public class TransferPrimaryOwnershipSaga : BaseSaga<TransferPrimaryOwnershipSaga, TransferPrimaryOwnershipData>,
        ISagaStartAction<TransferPrimaryOwnershipSagaInitializer>,
        ISagaAction<PrimaryOwnerTransferInitiated>,
        ISagaAction<InitiatePrimaryOwnerTransferRejected>,
        ISagaAction<PrimaryOwnershipTransferTokenGenerated>,
        ISagaAction<GeneratePrimaryOwnershipTransferTokenRejected>,
        ISagaAction<AcceptPrimaryOwnershipTransfer>,
        ISagaAction<RefusePrimaryOwnershipTransfer>,
        ISagaAction<PrimaryOwnershipTransfered>,
        ISagaAction<TransferPrimaryOwnershipRejected>,
        ISagaAction<CancelPrimaryOwnershipTransfer>
    {
        const string FORMER_OWNER_EVENT_NAME = "transfer-ownership-former-primary-owner";
        const string NEW_OWNER_EVENT_NAME = "transfer-ownership-new-primary-owner";
        private readonly IBusPublisher busPublisher;
        private readonly ILogger<TransferPrimaryOwnershipSaga> logger;
        private readonly ITenantsClient tenantsClient;
        private readonly IIdentityClient identityClient;

        public TransferPrimaryOwnershipSaga(IBusPublisher busPublisher, ITenantsClient tenantsClient, IIdentityClient identityClient, ILogger<TransferPrimaryOwnershipSaga> logger) : base(tenantsClient, logger)
        {
            this.busPublisher = busPublisher;
            this.tenantsClient = tenantsClient;
            this.identityClient = identityClient;
            this.logger = logger;
        }

        public Task CompensateAsync(TransferPrimaryOwnershipSagaInitializer message, ISagaContext context)
        {
            logger.LogInformation($"Calling compensate for {nameof(message)}.");
            busPublisher.Send(new SendPusherDomainUserCustomNotification(FORMER_OWNER_EVENT_NAME, "An error ocurred transfering the primary owner role.", Data.Domain, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        private async Task SetUserDataProperties(TransferPrimaryOwnershipSagaInitializer message)
        {
            try
            {
                Data.OwnerUser = await identityClient.GetUserAsync<User>(message.Domain, message.CommandingUserEmail);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to retrieve the Primary Owner when attempting Primary Ownership transfer.");
                await RejectAsync();
            }
            try
            {
                Data.TransferUser = await identityClient.GetUserAsync<User>(message.Domain, message.TransferUserEmail);
            }
            catch (HttpClientException<User> ex)
            {
                if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status404NotFound)
                {
                    logger.LogError(ex, $"The requested Transfer User '{message.TransferUserEmail}' was not found.");
                    await RejectAsync();
                }
                logger.LogError(ex, $"Failed to retrieve the requested Transfer User '{message.TransferUserEmail}'.");
                await RejectAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to retrieve the requested Transfer User '{message.TransferUserEmail}'.");
                await RejectAsync();
            }
        }

        public Task CompensateAsync(PrimaryOwnershipTransfered message, ISagaContext context)
        {
            logger.LogInformation($"Calling compensate for {nameof(message)}.");
            return Task.CompletedTask;
        }

        public Task HandleAsync(PrimaryOwnerTransferInitiated message, ISagaContext context)
        {
            this.busPublisher.Send(new GeneratePrimaryOwnershipTransferToken(Data.TransferUserEmail, Data.Domain), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public Task HandleAsync(PrimaryOwnershipTransfered message, ISagaContext context)
        {
            this.busPublisher.Send(new CompletePrimaryOwnerTransfer(Data.Domain, Data.Initiator, PrimaryOwnerTransferStateEnum.Accepted), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            busPublisher.Send(new SendPrimaryOwnerTransferAcceptedEmails(Data.TransferUserEmail, Data.OwnerUser.Email, Data.TransferUser.FirstName, Data.OwnerUser.FirstName, Data.OwnerUser.LastName, Data.Domain, Data.OrganizationName), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            busPublisher.Send(new SendPusherDomainUserCustomNotification(NEW_OWNER_EVENT_NAME, "The Primary Owner role was transfered successfully. Logout and log back in to receive your new permissions.", Data.Domain, Data.TransferUserEmail, OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            busPublisher.Send(new SendPusherDomainUserPredefinedNotification("ForceSignoutNotification", Data.Domain, Data.Initiator), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            busPublisher.Send(new SendPusherDomainUserCustomNotification(FORMER_OWNER_EVENT_NAME, "The Primary Owner role was transfered successfully.", Data.Domain, Data.Initiator, OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            Complete();
            return Task.CompletedTask;
        }

        public async Task HandleAsync(PrimaryOwnershipTransferTokenGenerated message, ISagaContext context)
        {
            var organizationNameModel = await tenantsClient.GetTenantAsync<TenantOrganizationNameModel>(Data.Domain).ConfigureAwait(false);
            Data.OrganizationName = organizationNameModel.OrganizationName;
            busPublisher.Send(new SendPrimaryOwnerTransferEmails(Data.TransferUserEmail, Data.OwnerUser.Email, Data.TransferUser.FirstName, Data.OwnerUser.FirstName, Data.OwnerUser.LastName, Data.Domain, organizationNameModel.OrganizationName, message.Token, context.SagaId), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
        }

        public Task CompensateAsync(PrimaryOwnershipTransferTokenGenerated message, ISagaContext context)
        {
            logger.LogInformation($"Calling compensate for {nameof(message)}.");
            return Task.CompletedTask;
        }

        public async Task HandleAsync(GeneratePrimaryOwnershipTransferTokenRejected message, ISagaContext context)
        {
            await RejectAsync();
        }

        public Task CompensateAsync(GeneratePrimaryOwnershipTransferTokenRejected message, ISagaContext context)
        {
            logger.LogInformation($"Calling compensate for {nameof(message)}.");
            return Task.CompletedTask;
        }

        public Task HandleAsync(AcceptPrimaryOwnershipTransfer message, ISagaContext context)
        {
            busPublisher.Send(new TransferPrimaryOwnership(Data.Initiator, Data.TransferUserEmail, Data.Domain, message.Token), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public Task CompensateAsync(AcceptPrimaryOwnershipTransfer message, ISagaContext context)
        {
            logger.LogInformation($"Calling compensate for {nameof(message)}.");
            return Task.CompletedTask;
        }

        public Task HandleAsync(RefusePrimaryOwnershipTransfer message, ISagaContext context)
        {
            busPublisher.Send(new CompletePrimaryOwnerTransfer(Data.Domain, Data.Initiator, PrimaryOwnerTransferStateEnum.Refused), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            busPublisher.Send(new SendPrimaryOwnerTransferRefusedEmails(Data.TransferUserEmail, Data.OwnerUser.Email, Data.TransferUser.FirstName, Data.OwnerUser.FirstName, Data.OwnerUser.LastName, Data.Domain, Data.OrganizationName), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            busPublisher.Send(new SendPusherDomainUserCustomNotification(FORMER_OWNER_EVENT_NAME, "The Primary Owner role was refused by the recipient.", Data.Domain, Data.Initiator, OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            Complete();
            return Task.CompletedTask;
        }

        public Task CompensateAsync(RefusePrimaryOwnershipTransfer message, ISagaContext context)
        {
            logger.LogInformation($"Calling compensate for {nameof(message)}.");
            return Task.CompletedTask;
        }

        public async Task HandleAsync(TransferPrimaryOwnershipSagaInitializer message, ISagaContext context)
        {
            await SetUserDataProperties(message);
            Data.DatabaseUsername = await GetPgsqlDatabaseUsernameOrReject(message);
            Data.Domain = message.Domain;
            Data.Initiator = message.CommandingUserEmail;
            Data.TransferUserEmail = message.TransferUserEmail;
            busPublisher.Send(new InitiatePrimaryOwnerTransfer(message.Domain, message.CommandingUserEmail, message.TransferUserEmail), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
        }

        public Task CompensateAsync(PrimaryOwnerTransferInitiated message, ISagaContext context)
        {
            busPublisher.Send(new CompletePrimaryOwnerTransfer(Data.Domain, "System", PrimaryOwnerTransferStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public Task HandleAsync(InitiatePrimaryOwnerTransferRejected message, ISagaContext context)
        {
            Reject();
            busPublisher.Send(new SendPusherDomainUserCustomNotification(FORMER_OWNER_EVENT_NAME, $"An error occured transfering the Primary Owner role: {message.Reason}", Data.Domain, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public Task CompensateAsync(InitiatePrimaryOwnerTransferRejected message, ISagaContext context)
        {
            logger.LogInformation($"Calling compensate for {nameof(message)}.");
            return Task.CompletedTask;
        }

        public Task HandleAsync(TransferPrimaryOwnershipRejected message, ISagaContext context)
        {
            Reject();
            busPublisher.Send(new SendPusherDomainUserCustomNotification(FORMER_OWNER_EVENT_NAME, $"An error occured transfering the Primary Owner role: {message.Reason}", Data.Domain, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public Task CompensateAsync(TransferPrimaryOwnershipRejected message, ISagaContext context)
        {
            logger.LogInformation($"Calling compensate for {nameof(message)}.");
            return Task.CompletedTask;
        }

        public Task HandleAsync(CancelPrimaryOwnershipTransfer message, ISagaContext context)
        {
            busPublisher.Send(new SendPrimaryOwnerTransferCancelledEmails(Data.TransferUserEmail, Data.OwnerUser.Email, Data.TransferUser.FirstName, Data.OwnerUser.FirstName, Data.OwnerUser.LastName, Data.Domain, Data.OrganizationName), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            busPublisher.Send(new SendPusherDomainUserCustomNotification(FORMER_OWNER_EVENT_NAME, "The Primary Owner role transfer was cancelled.", Data.Domain, Data.Initiator, OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            busPublisher.Send(new CompletePrimaryOwnerTransfer(Data.Domain, Data.OwnerUser.Email, PrimaryOwnerTransferStateEnum.Cancelled), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            Complete();
            return Task.CompletedTask;
        }

        public Task CompensateAsync(CancelPrimaryOwnershipTransfer message, ISagaContext context)
        {
            logger.LogInformation($"Calling compensate for {nameof(message)}.");
            return Task.CompletedTask;
        }
    }

    public class TransferPrimaryOwnershipData : BaseSagaData
    {
        public User TransferUser { get; set; }
        public User OwnerUser { get; set; }
        public string OrganizationName { get; set; }
        public string TransferUserEmail { get; set; }
    }
}
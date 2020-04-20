using System;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
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
    public class TransferPrimaryOwnershipSaga : Saga<TransferPrimaryOwnershipData>,
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
        private readonly IdentityHttpClient identityClient;

        public TransferPrimaryOwnershipSaga(IBusPublisher busPublisher, IdentityHttpClient identityClient, ILogger<TransferPrimaryOwnershipSaga> logger)
        {
            this.busPublisher = busPublisher;
            this.identityClient = identityClient;
            this.logger = logger;
        }

        public Task CompensateAsync(TransferPrimaryOwnershipSagaInitializer message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            busPublisher.Send(new SendPusherDomainUserCustomNotification(FORMER_OWNER_EVENT_NAME, "An error ocurred transfering the primary owner role.", Data.TenantId, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        private async Task SetUserDataProperties(TransferPrimaryOwnershipSagaInitializer message)
        {
            try
            {
                RangerApiResponse<User> apiResponse = null;
                try
                {
                    apiResponse = await identityClient.GetUserAsync<User>(message.TenantId, message.CommandingUserEmail);
                }
                catch (ApiException ex)
                {
                    logger.LogError(ex, "Failed to retrieve the Primary Owner when attempting Primary Ownership transfer.");
                    await RejectAsync();
                }
                Data.OwnerUser = apiResponse.Result;

                RangerApiResponse<User> apiResponse1 = null;
                try
                {
                    apiResponse1 = await identityClient.GetUserAsync<User>(message.TenantId, message.TransferUserEmail);
                }
                catch (ApiException ex)
                {
                    logger.LogError(ex, $"Failed to retrieve the requested Transfer User '{message.TransferUserEmail}'.");
                    await RejectAsync();
                }
                Data.TransferUser = apiResponse1.Result;
            }
            catch (ApiException)
            {
                logger.LogError($"Failed to retrieve the requested Transfer User '{message.TransferUserEmail}'.");
                await RejectAsync();
            }
        }

        public Task CompensateAsync(PrimaryOwnershipTransfered message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            return Task.CompletedTask;
        }

        public Task HandleAsync(PrimaryOwnerTransferInitiated message, ISagaContext context)
        {
            this.busPublisher.Send(new GeneratePrimaryOwnershipTransferToken(Data.TransferUserEmail, Data.TenantId), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public Task HandleAsync(PrimaryOwnershipTransfered message, ISagaContext context)
        {
            this.busPublisher.Send(new CompletePrimaryOwnerTransfer(Data.TenantId, Data.Initiator, PrimaryOwnerTransferStateEnum.Accepted), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            busPublisher.Send(new SendPrimaryOwnerTransferAcceptedEmails(Data.TransferUserEmail, Data.OwnerUser.Email, Data.TransferUser.FirstName, Data.OwnerUser.FirstName, Data.OwnerUser.LastName, Data.TenantId), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            busPublisher.Send(new SendPusherDomainUserCustomNotification(NEW_OWNER_EVENT_NAME, "The Primary Owner role was transfered successfully. Logout and log back in to receive your new permissions.", Data.TenantId, Data.TransferUserEmail, OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            busPublisher.Send(new SendPusherDomainUserPredefinedNotification("ForceSignoutNotification", Data.TenantId, Data.Initiator), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            busPublisher.Send(new SendPusherDomainUserCustomNotification(FORMER_OWNER_EVENT_NAME, "The Primary Owner role was transfered successfully.", Data.TenantId, Data.Initiator, OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            Complete();
            return Task.CompletedTask;
        }

        public Task HandleAsync(PrimaryOwnershipTransferTokenGenerated message, ISagaContext context)
        {
            busPublisher.Send(new SendPrimaryOwnerTransferEmails(Data.TransferUserEmail, Data.OwnerUser.Email, Data.TransferUser.FirstName, Data.OwnerUser.FirstName, Data.OwnerUser.LastName, Data.TenantId, message.Token, context.SagaId), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public Task CompensateAsync(PrimaryOwnershipTransferTokenGenerated message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            return Task.CompletedTask;
        }

        public async Task HandleAsync(GeneratePrimaryOwnershipTransferTokenRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            await RejectAsync();
        }

        public Task CompensateAsync(GeneratePrimaryOwnershipTransferTokenRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            return Task.CompletedTask;
        }

        public Task HandleAsync(AcceptPrimaryOwnershipTransfer message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            busPublisher.Send(new TransferPrimaryOwnership(Data.Initiator, Data.TransferUserEmail, Data.TenantId, message.Token), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public Task CompensateAsync(AcceptPrimaryOwnershipTransfer message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            return Task.CompletedTask;
        }

        public Task HandleAsync(RefusePrimaryOwnershipTransfer message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            busPublisher.Send(new CompletePrimaryOwnerTransfer(Data.TenantId, Data.Initiator, PrimaryOwnerTransferStateEnum.Refused), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            busPublisher.Send(new SendPrimaryOwnerTransferRefusedEmails(Data.TransferUserEmail, Data.OwnerUser.Email, Data.TransferUser.FirstName, Data.OwnerUser.FirstName, Data.OwnerUser.LastName, Data.TenantId), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            busPublisher.Send(new SendPusherDomainUserCustomNotification(FORMER_OWNER_EVENT_NAME, "The Primary Owner role was refused by the recipient.", Data.TenantId, Data.Initiator, OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            Complete();
            return Task.CompletedTask;
        }

        public Task CompensateAsync(RefusePrimaryOwnershipTransfer message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            return Task.CompletedTask;
        }

        public async Task HandleAsync(TransferPrimaryOwnershipSagaInitializer message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            await SetUserDataProperties(message);
            Data.TenantId = message.TenantId;
            Data.Initiator = message.CommandingUserEmail;
            Data.TransferUserEmail = message.TransferUserEmail;
            busPublisher.Send(new InitiatePrimaryOwnerTransfer(message.TenantId, message.CommandingUserEmail, message.TransferUserEmail), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
        }

        public Task CompensateAsync(PrimaryOwnerTransferInitiated message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            busPublisher.Send(new CompletePrimaryOwnerTransfer(Data.TenantId, "System", PrimaryOwnerTransferStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public Task HandleAsync(InitiatePrimaryOwnerTransferRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            Reject();
            busPublisher.Send(new SendPusherDomainUserCustomNotification(FORMER_OWNER_EVENT_NAME, $"An error occured transfering the Primary Owner role: {message.Reason}", Data.TenantId, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public Task CompensateAsync(InitiatePrimaryOwnerTransferRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            return Task.CompletedTask;
        }

        public Task HandleAsync(TransferPrimaryOwnershipRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            Reject();
            busPublisher.Send(new SendPusherDomainUserCustomNotification(FORMER_OWNER_EVENT_NAME, $"An error occured transfering the Primary Owner role: {message.Reason}", Data.TenantId, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public Task CompensateAsync(TransferPrimaryOwnershipRejected message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            return Task.CompletedTask;
        }

        public Task HandleAsync(CancelPrimaryOwnershipTransfer message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'.");
            busPublisher.Send(new SendPrimaryOwnerTransferCancelledEmails(Data.TransferUserEmail, Data.OwnerUser.Email, Data.TransferUser.FirstName, Data.OwnerUser.FirstName, Data.OwnerUser.LastName, Data.TenantId), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            busPublisher.Send(new SendPusherDomainUserCustomNotification(FORMER_OWNER_EVENT_NAME, "The Primary Owner role transfer was cancelled.", Data.TenantId, Data.Initiator, OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            busPublisher.Send(new CompletePrimaryOwnerTransfer(Data.TenantId, Data.OwnerUser.Email, PrimaryOwnerTransferStateEnum.Cancelled), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            Complete();
            return Task.CompletedTask;
        }

        public Task CompensateAsync(CancelPrimaryOwnershipTransfer message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'.");
            return Task.CompletedTask;
        }
    }

    public class TransferPrimaryOwnershipData : BaseSagaData
    {
        public User TransferUser { get; set; }
        public User OwnerUser { get; set; }
        public string TransferUserEmail { get; set; }
    }
}
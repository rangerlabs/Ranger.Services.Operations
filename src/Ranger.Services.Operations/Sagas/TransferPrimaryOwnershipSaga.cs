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

namespace Ranger.Services.Operations.Sagas
{
    public class TransferPrimaryOwnershipSaga : BaseSaga<TransferPrimaryOwnershipSaga, TransferPrimaryOwnershipData>,
        ISagaStartAction<TransferPrimaryOwnershipSagaInitializer>,
        ISagaAction<PrimaryOwnershipTransferTokenGenerated>,
        ISagaAction<GeneratePrimaryOwnershipTransferTokenRejected>,
        ISagaAction<PrimaryOwnershipTransferAccepted>,
        ISagaAction<PrimaryOwnershipTransferRefused>,
        ISagaAction<PrimaryOwnershipTransfered>
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

        public async Task CompensateAsync(TransferPrimaryOwnershipSagaInitializer message, ISagaContext context)
        {
            await SetUserDataProperties(message);
            Data.DatabaseUsername = (await tenantsClient.GetTenantAsync<ContextTenant>(message.Domain)).DatabaseUsername;
            Data.Domain = message.Domain;
            Data.Initiator = message.CommandingUserEmail;
            Data.TransferUserEmail = message.TransferUserEmail;
            await Task.Run(() => this.busPublisher.Send(new GeneratePrimaryOwnershipTransferToken(message.CommandingUserEmail, message.Domain), CorrelationContext.FromId(Guid.Parse(context.SagaId))));
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

        public async Task CompensateAsync(PrimaryOwnershipTransfered message, ISagaContext context)
        {
            await Task.Run(() =>
               logger.LogInformation("Calling compensate for PrimaryOwnershipTransfered.")
            );
        }

        public async Task HandleAsync(TransferPrimaryOwnershipSagaInitializer message, ISagaContext context)
        {
            await SetUserDataProperties(message);
            Data.DatabaseUsername = (await tenantsClient.GetTenantAsync<ContextTenant>(message.Domain)).DatabaseUsername;
            Data.Domain = message.Domain;
            Data.Initiator = message.CommandingUserEmail;
            Data.TransferUserEmail = message.TransferUserEmail;
            await Task.Run(() => this.busPublisher.Send(new GeneratePrimaryOwnershipTransferToken(message.CommandingUserEmail, message.Domain), CorrelationContext.FromId(Guid.Parse(context.SagaId))));
        }

        public async Task HandleAsync(PrimaryOwnershipTransfered message, ISagaContext context)
        {
            await Task.Run(() =>
            {
                busPublisher.Send(new SendPusherDomainUserCustomNotification(NEW_OWNER_EVENT_NAME, "The Primary Owner role was transfered successfully. Logout and log back in to receive your new permissions.", Data.Domain, Data.TransferUserEmail, OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                busPublisher.Send(new SendPusherDomainUserPredefinedNotification("ForceSignoutNotification", Data.Domain, Data.Initiator), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
                busPublisher.Send(new SendPusherDomainUserCustomNotification(FORMER_OWNER_EVENT_NAME, "The Primary Owner role was transfered successfully.", Data.Domain, Data.Initiator, OperationsStateEnum.Completed), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            });
            await CompleteAsync();
        }

        public async Task HandleAsync(PrimaryOwnershipTransferTokenGenerated message, ISagaContext context)
        {
            var organizationNameModel = await tenantsClient.GetTenantAsync<TenantOrganizationNameModel>(Data.Domain).ConfigureAwait(false);
            await Task.Run(() => busPublisher.Send(new SendPrimaryOwnerTransferEmails(Data.TransferUserEmail, Data.OwnerUser.Email, Data.TransferUser.FirstName, Data.OwnerUser.FirstName, Data.OwnerUser.LastName, Data.Domain, organizationNameModel.OrganizationName, message.Token, context.SagaId), CorrelationContext.FromId(Guid.Parse(context.SagaId))));
        }

        public async Task CompensateAsync(PrimaryOwnershipTransferTokenGenerated message, ISagaContext context)
        {
            await Task.Run(() =>
               logger.LogInformation("Calling compensate for TransferPrimaryOwnershipSagaInitializer.")
            );
        }

        public async Task HandleAsync(GeneratePrimaryOwnershipTransferTokenRejected message, ISagaContext context)
        {
            await RejectAsync();
        }

        public async Task CompensateAsync(GeneratePrimaryOwnershipTransferTokenRejected message, ISagaContext context)
        {
            await Task.Run(() =>
                logger.LogInformation("Calling compensate for GeneratePrimaryOwnershipTransferTokenRejected.")
            );
        }

        public async Task HandleAsync(PrimaryOwnershipTransferAccepted message, ISagaContext context)
        {
            await Task.Run(() => busPublisher.Send(new TransferPrimaryOwnership(Data.Initiator, Data.TransferUserEmail, Data.Domain, message.Token), CorrelationContext.FromId(Guid.Parse(context.SagaId))));
        }

        public async Task CompensateAsync(PrimaryOwnershipTransferAccepted message, ISagaContext context)
        {
            await Task.Run(() =>
                logger.LogInformation("Calling compensate for PrimaryOwnershipTransferAccepted.")
            );
        }

        public async Task HandleAsync(PrimaryOwnershipTransferRefused message, ISagaContext context)
        {
            busPublisher.Send(new SendPusherDomainUserCustomNotification(FORMER_OWNER_EVENT_NAME, "The Primary Owner role was rejected by the recipient.", Data.Domain, Data.Initiator, OperationsStateEnum.Rejected), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            await CompleteAsync();
        }

        public async Task CompensateAsync(PrimaryOwnershipTransferRefused message, ISagaContext context)
        {
            await Task.Run(() =>
                logger.LogInformation("Calling compensate for PrimaryOwnershipTransferRefused.")
            );
        }
    }

    public class TransferPrimaryOwnershipData : BaseSagaData
    {
        public User TransferUser { get; set; }
        public User OwnerUser { get; set; }
        public string TransferUserEmail { get; set; }
    }
}
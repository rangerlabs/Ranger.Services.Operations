using System;
using System.Threading.Tasks;
using Chronicle;
using Microsoft.Extensions.Logging;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;
using Ranger.Services.Operations;
using Ranger.Services.Operations.Messages.Identity.Commands;

namespace Ranger.Services.Operations.Sagas
{
    public class TransferPrimaryOwnershipSaga : BaseSaga<TransferPrimaryOwnershipSaga, TransferPrimaryOwnershipData>,
        ISagaStartAction<TransferPrimaryOwnershipSagaInitializer>,
        ISagaAction<PrimaryOwnershipTransfered>
    {
        const string EVENT_NAME = "transfer-primary-ownership";
        private readonly IBusPublisher busPublisher;
        private readonly ILogger<TransferPrimaryOwnershipSaga> logger;
        private readonly ITenantsClient tenantsClient;

        public TransferPrimaryOwnershipSaga(IBusPublisher busPublisher, ITenantsClient tenantsClient, ILogger<TransferPrimaryOwnershipSaga> logger) : base(tenantsClient, logger)
        {
            this.busPublisher = busPublisher;
            this.tenantsClient = tenantsClient;
            this.logger = logger;
        }

        public async Task CompensateAsync(TransferPrimaryOwnershipSagaInitializer message, ISagaContext context)
        {
            Data.Domain = message.Domain;
            Data.Initiator = message.CommandingUserEmail;
            Data.TransferUserEmail = message.TransferUserEmail;
            await Task.Run(() => this.busPublisher.Send(new TransferPrimaryOwnership(message.CommandingUserEmail, message.TransferUserEmail, message.Domain), CorrelationContext.FromId(Guid.Parse(context.SagaId))));
        }

        public async Task CompensateAsync(PrimaryOwnershipTransfered message, ISagaContext context)
        {
            await Task.Run(() =>
               logger.LogInformation("Calling compensate for PrimaryOwnershipTransfered.")
            );
        }

        public async Task HandleAsync(TransferPrimaryOwnershipSagaInitializer message, ISagaContext context)
        {
            await Task.Run(() =>
               logger.LogInformation("Calling compensate for TransferPrimaryOwnershipSagaInitializer.")
            );
        }

        public async Task HandleAsync(PrimaryOwnershipTransfered message, ISagaContext context)
        {

        }
    }

    public class TransferPrimaryOwnershipData : BaseSagaData
    {
        public string TransferUserEmail { get; set; }
    }
}
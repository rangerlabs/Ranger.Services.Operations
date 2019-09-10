using System.Threading.Tasks;
using Chronicle;
using Microsoft.Extensions.Logging;
using Ranger.RabbitMQ;
using Ranger.Services.Operations.Data;

namespace Ranger.Services.Operations
{
    public class GenericEventHandler<TEvent> : IEventHandler<TEvent>
        where TEvent : class, IEvent
    {
        private readonly ISagaCoordinator sagaCoordinator;
        private readonly IBusPublisher busPublisher;
        private readonly ISagaStateRepository repository;
        private readonly ILogger<GenericEventHandler<TEvent>> logger;

        public GenericEventHandler(ISagaCoordinator sagaCoordinator, IBusPublisher busPublisher, ISagaStateRepository repository, ILogger<GenericEventHandler<TEvent>> logger)
        {
            this.sagaCoordinator = sagaCoordinator;
            this.busPublisher = busPublisher;
            this.repository = repository;
            this.logger = logger;
        }
        public async Task HandleAsync(TEvent @event, ICorrelationContext context)
        {
            if (@event.BelongsToSaga())
            {
                var sagaContext = SagaContext.FromCorrelationContext(context);

                try
                {
                    await sagaCoordinator.ProcessAsync(@event, context: sagaContext);
                }
                catch (ChronicleException ex)
                {
                    logger.LogError(sagaContext.SagaContextError.Exception, "An exception was thrown resulting in a saga rejection. Ack'ing message.");
                }
            }

            // switch (@event)
            // {
            //     case IRejectedEvent rejectedSingleEvent:
            //         busPublisher.Send<SendPusherPrivateFrontendNotification>(
            //             new SendPusherPrivateFrontendNotification(
            //                 rejectedSingleEvent.GetType().Name,
            //                 context.Domain,
            //                 context.UserEmail,
            //                 OperationsStateEnum.Rejected),
            //             context);
            //         return;
            //     case IEvent completedSingleEvent:
            //         busPublisher.Send<SendPusherPrivateFrontendNotification>(
            //             new SendPusherPrivateFrontendNotification(
            //                 completedSingleEvent.GetType().Name,
            //                 context.Domain,
            //                 context.UserEmail,
            //                 OperationsStateEnum.Rejected),
            //             context);
            //         return;
            // }
        }
    }
}
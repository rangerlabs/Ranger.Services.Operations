using System.Threading.Tasks;
using Chronicle;
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

        public GenericEventHandler(ISagaCoordinator sagaCoordinator, IBusPublisher busPublisher, ISagaStateRepository repository)
        {
            this.sagaCoordinator = sagaCoordinator;
            this.busPublisher = busPublisher;
            this.repository = repository;
        }
        public async Task HandleAsync(TEvent @event, ICorrelationContext context)
        {
            if (@event.BelongsToSaga())
            {
                var sagaContext = SagaContext.FromCorrelationContext(context);
                await sagaCoordinator.ProcessAsync(@event, context: sagaContext);
            }

            switch (@event)
            {
                case IRejectedEvent rejectedSingleEvent:
                    busPublisher.Publish<SendPusherFrontendNotification>(
                        new SendPusherFrontendNotification(
                            rejectedSingleEvent.GetType().Name,
                            context.Domain,
                            context.UserEmail,
                            OperationsStateEnum.Rejected),
                        context);
                    return;
                case IEvent completedSingleEvent:
                    busPublisher.Publish<SendPusherFrontendNotification>(
                        new SendPusherFrontendNotification(
                            completedSingleEvent.GetType().Name,
                            context.Domain,
                            context.UserEmail,
                            OperationsStateEnum.Rejected),
                        context);
                    return;
            }
        }
    }
}
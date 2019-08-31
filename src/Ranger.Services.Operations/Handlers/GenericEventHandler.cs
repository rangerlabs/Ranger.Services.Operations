using System.Threading.Tasks;
using Chronicle;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations
{
    public class GenericEventHandler<TEvent> : IEventHandler<TEvent>
        where TEvent : class, IEvent
    {
        private readonly ISagaCoordinator sagaCoordinator;

        public GenericEventHandler(ISagaCoordinator sagaCoordinator, IOperationsPublisher operationPublisher, IOperationsStorage operationsStorage)
        {
            this.sagaCoordinator = sagaCoordinator;
        }
        public async Task HandleAsync(TEvent @event, ICorrelationContext context)
        {
            if (@event.BelongsToSaga())
            {
                var sagaContext = SagaContext.FromCorrelationContext(context);
                await sagaCoordinator.ProcessAsync(@event, context: sagaContext);
            }
        }
    }
}
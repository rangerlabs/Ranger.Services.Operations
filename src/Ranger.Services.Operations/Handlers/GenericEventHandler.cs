using System;
using System.Threading.Tasks;
using Chronicle;
using Microsoft.Extensions.Logging;
using Ranger.RabbitMQ;
using Ranger.RabbitMQ.BusPublisher;

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
                logger.LogDebug("Event {EventName} was found to be an action in one or more sagas", @event.GetType().Name);
                var sagaContext = SagaContext.FromCorrelationContext(context);
                try
                {
                    await sagaCoordinator.ProcessAsync(@event, context: sagaContext);
                }
                catch (NotImplementedException ex)
                {
                    logger.LogError(ex, "A Chronicle method was not implemented");
                }
                catch (ChronicleException)
                {
                    logger.LogError(sagaContext.SagaContextError.Exception, "An exception was thrown resulting in a saga rejection. Ack'ing message");
                }
            }
            else {
                logger.LogDebug("Event {EventName} is not an action in any sagas", @event.GetType().Name);
            }
        }
    }
}
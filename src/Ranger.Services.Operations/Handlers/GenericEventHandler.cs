using System.Threading.Tasks;
using Chronicle;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations {
    public class GenericEventHandler<TEvent> : IEventHandler<TEvent>
        where TEvent : class, IEvent {
            private readonly ISagaCoordinator sagaCoordinator;

            public GenericEventHandler (ISagaCoordinator sagaCoordinator) {
                this.sagaCoordinator = sagaCoordinator;
            }
            public async Task HandleAsync (TEvent command) {
                if (!command.BelongsToSaga ()) {
                    return;
                }

                var sagaContext = SagaContext.FromCorrelationContext (command.CorrelationContext);
                await sagaCoordinator.ProcessAsync (command, context : sagaContext);
            }
        }
}
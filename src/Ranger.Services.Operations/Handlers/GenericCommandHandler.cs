using System.Threading.Tasks;
using Chronicle;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations {
    public class GenericCommandHandler<TCommand> : ICommandHandler<TCommand>
        where TCommand : class, ICommand {
            private readonly ISagaCoordinator sagaCoordinator;

            public GenericCommandHandler (ISagaCoordinator sagaCoordinator) {
                this.sagaCoordinator = sagaCoordinator;
            }
            public async Task HandleAsync (TCommand command) {
                if (!command.BelongsToSaga ()) {
                    return;
                }

                var sagaContext = SagaContext.FromCorrelationContext (command.CorrelationContext);
                await sagaCoordinator.ProcessAsync (command, context : sagaContext);
            }
        }
}
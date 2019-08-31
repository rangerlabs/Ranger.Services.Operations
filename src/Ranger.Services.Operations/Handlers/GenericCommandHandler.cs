using System.Threading.Tasks;
using Chronicle;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations
{
    public class GenericCommandHandler<TCommand> : ICommandHandler<TCommand>
        where TCommand : class, ICommand
    {
        private readonly ISagaCoordinator sagaCoordinator;

        public GenericCommandHandler(ISagaCoordinator sagaCoordinator)
        {
            this.sagaCoordinator = sagaCoordinator;
        }
        public async Task HandleAsync(TCommand command, ICorrelationContext context)
        {
            if (!command.BelongsToSaga())
            {
                return;
            }

            var sagaContext = SagaContext.FromCorrelationContext(context);
            await sagaCoordinator.ProcessAsync(command, context: sagaContext);
        }
    }
}
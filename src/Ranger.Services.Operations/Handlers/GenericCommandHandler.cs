using System.Threading.Tasks;
using Chronicle;
using Microsoft.Extensions.Logging;
using Ranger.RabbitMQ;
using System;

namespace Ranger.Services.Operations
{
    public class GenericCommandHandler<TCommand> : ICommandHandler<TCommand>
        where TCommand : class, ICommand
    {
        private readonly ISagaCoordinator sagaCoordinator;
        private readonly ILogger<GenericCommandHandler<TCommand>> logger;

        public GenericCommandHandler(ISagaCoordinator sagaCoordinator, ILogger<GenericCommandHandler<TCommand>> logger)
        {
            this.sagaCoordinator = sagaCoordinator;
            this.logger = logger;
        }
        public async Task HandleAsync(TCommand command, ICorrelationContext context)
        {
            if (!command.BelongsToSaga())
            {
                return;
            }

            var sagaContext = SagaContext.FromCorrelationContext(context);
            try
            {
                await sagaCoordinator.ProcessAsync(command, context: sagaContext);
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
    }
}
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Operations
{
    public abstract class SagaInitializer
    {
        public string Domain { get; protected set; }
    }
}
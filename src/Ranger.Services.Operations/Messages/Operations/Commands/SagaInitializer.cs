using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Operations
{
    public abstract class SagaInitializer
    {
        public string TenantId { get; protected set; }
    }
}
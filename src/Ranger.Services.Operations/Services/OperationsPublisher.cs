using System.Threading.Tasks;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations {
    public class OperationPublisher : IOperationPublisher {
        private readonly IBusPublisher _busPublisher;

        public OperationPublisher (IBusPublisher busPublisher) {
            _busPublisher = busPublisher;
        }

        public void Pending (CorrelationContext context) => _busPublisher.Publish (new OperationPending (context));
        public void Complete (CorrelationContext context) => _busPublisher.Publish (new OperationCompleted (context));
        public void Reject (CorrelationContext context, string message, string code) => _busPublisher.Publish (new OperationRejected (context, message, code));
    }
}
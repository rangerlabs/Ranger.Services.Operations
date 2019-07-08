using System.Threading.Tasks;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations {
    public interface IOperationPublisher {
        void Pending (CorrelationContext context);
        void Complete (CorrelationContext context);
        void Reject (CorrelationContext context, string code, string message);
    }
}
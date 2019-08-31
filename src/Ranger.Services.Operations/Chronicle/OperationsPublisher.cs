using System.Threading.Tasks;
using Chronicle;
using PusherServer;
using Ranger.RabbitMQ;
using Ranger.Services.Operations.Data;

namespace Ranger.Services.Operations
{
    public class OperationsPublisher : IOperationsPublisher
    {
        private readonly IPusher pusher;

        public OperationsPublisher(IPusher pusher)
        {
            this.pusher = pusher;
        }

        public async Task SendRangerLabsStatusUpdate(ISagaContext context, string domain, OperationsStateEnum state)
        {
            var result = await pusher.TriggerAsync(
                "ranger-labs",
                "registration-event",
                new { domain = domain, correlationId = context.SagaId.Id, status = state }
            );
        }
    }
}
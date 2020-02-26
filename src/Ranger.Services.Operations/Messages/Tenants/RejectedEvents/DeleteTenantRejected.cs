using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Tenants.RejectedEvents
{
    public class DeleteTenantRejected : IRejectedEvent
    {
        public string Reason { get; }
        public string Code { get; }

        public DeleteTenantRejected(string message, string code)
        {
            this.Reason = message;
            this.Code = code;
        }
    }
}
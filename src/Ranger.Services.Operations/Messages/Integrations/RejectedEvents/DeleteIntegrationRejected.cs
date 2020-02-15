using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Integrations
{
    [MessageNamespaceAttribute("integrations")]
    public class DeleteIntegrationRejected : IRejectedEvent
    {
        public string Reason { get; }
        public string Code { get; }

        public DeleteIntegrationRejected(string reason, string code)
        {
            this.Reason = reason;
            this.Code = code;
        }
    }
}
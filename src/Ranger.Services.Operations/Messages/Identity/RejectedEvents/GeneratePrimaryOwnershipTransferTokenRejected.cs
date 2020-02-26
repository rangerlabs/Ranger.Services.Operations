using Ranger.RabbitMQ;

namespace Ranger.Services.Operations
{
    [MessageNamespaceAttribute("identity")]
    public class GeneratePrimaryOwnershipTransferTokenRejected : IRejectedEvent
    {
        public string Reason { get; }
        public string Code { get; }

        public GeneratePrimaryOwnershipTransferTokenRejected(string reason, string code)
        {
            this.Reason = reason;
            this.Code = code;
        }
    }
}
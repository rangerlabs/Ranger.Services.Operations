using Ranger.RabbitMQ;

namespace Ranger.Services.Operations
{
    [MessageNamespaceAttribute("identity")]
    public class DeleteUserRejected : IRejectedEvent
    {
        public DeleteUserRejected(string reason, string code)
        {
            this.Reason = reason;
            this.Code = code;
        }
        public string Reason { get; set; }
        public string Code { get; set; }
    }
}
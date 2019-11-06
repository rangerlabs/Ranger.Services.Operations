using Ranger.RabbitMQ;

namespace Ranger.Services.Operations
{
    [MessageNamespaceAttribute("idenity")]
    public class CreateApplicationUserRejected : IRejectedEvent
    {
        public CreateApplicationUserRejected(string reason, string code)
        {
            this.Reason = reason;
            this.Code = code;
        }
        public string Reason { get; set; }
        public string Code { get; set; }
    }
}
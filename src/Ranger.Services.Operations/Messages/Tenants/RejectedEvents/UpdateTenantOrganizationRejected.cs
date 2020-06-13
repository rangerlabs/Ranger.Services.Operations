using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Tenants.RejectedEvents
{
    [MessageNamespace("tenants")]
    public class UpdateTenantOrganizationRejected : IRejectedEvent
    {
        public UpdateTenantOrganizationRejected(string reason, string code)
        {
            this.Reason = reason;
            this.Code = code;

        }
        public string Reason { get; }
        public string Code { get; }
    }
}
using Ranger.Common;
using Ranger.RabbitMQ;

namespace Ranger.Services.Subscriptions
{
    [MessageNamespace("identity")]
    public class EnforceAccountResourceLimits : ICommand
    {
        public string TenantId;
        public int Limit;
        public EnforceAccountResourceLimits(string tenantId, int limit)
        {
            this.Limit = limit;
            this.TenantId = tenantId;
        }
    }
}
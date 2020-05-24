using System.Collections.Generic;
using Ranger.Common;
using Ranger.RabbitMQ;

namespace Ranger.Services.Subscriptions
{
    [MessageNamespace("operations")]
    public class EnforceSubscriptions : ICommand
    {
        public IEnumerable<string> TenantIds { get; set; }

        public EnforceSubscriptions(IEnumerable<string> tenantIds)
        {
            this.TenantIds = tenantIds;
        }
    }
}
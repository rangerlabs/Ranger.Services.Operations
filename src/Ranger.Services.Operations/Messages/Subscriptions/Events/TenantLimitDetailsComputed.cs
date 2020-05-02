using System.Collections.Generic;
using Ranger.RabbitMQ;
using Ranger.Services.Operations;

namespace Ranger.Services.Subscriptions.Messages.Events
{
    [MessageNamespace("subscriptions")]
    public class TenantLimitDetailsComputed : IEvent
    {
        public IEnumerable<(string tenantId, LimitFields limits)> TenantLimitDetails { get; set; }

        public TenantLimitDetailsComputed(IEnumerable<(string tenantId, LimitFields limits)> tenantLimitDetails)
        {
            TenantLimitDetails = tenantLimitDetails;
        }
    }
}
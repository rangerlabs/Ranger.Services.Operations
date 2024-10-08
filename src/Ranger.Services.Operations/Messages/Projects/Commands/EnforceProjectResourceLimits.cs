using System.Collections.Generic;
using Ranger.Common;
using Ranger.RabbitMQ;

namespace Ranger.Services.Subscriptions
{
    [MessageNamespace("projects")]
    public class EnforceProjectResourceLimits : ICommand
    {
        public IEnumerable<(string, int)> TenantLimits { get; }

        public EnforceProjectResourceLimits(IEnumerable<(string, int)> tenantLimits)
        {
            this.TenantLimits = tenantLimits;
        }
    }
}
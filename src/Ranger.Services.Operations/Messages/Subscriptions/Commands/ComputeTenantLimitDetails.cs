using System.Collections.Generic;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Subscriptions.Commands
{
    [MessageNamespace("subscriptions")]
    public class ComputeTenantLimitDetails : ICommand
    {
        public IEnumerable<string> TenantIds { get; set; }

        public ComputeTenantLimitDetails(IEnumerable<string> tenantIds)
        {
            TenantIds = tenantIds;
        }
    }
}
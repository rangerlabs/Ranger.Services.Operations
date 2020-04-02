using System;
using Ranger.Common;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Subscriptions
{
    [MessageNamespace("subscriptions")]
    public class IncrementResourceCount : ICommand
    {
        public string Domain { get; }
        public ResourceEnum Resource { get; }

        public IncrementResourceCount(string domain, ResourceEnum resource)
        {
            if (String.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} was null or whitespace.");
            }
            this.Domain = domain;
            this.Resource = resource;
        }
    }
}
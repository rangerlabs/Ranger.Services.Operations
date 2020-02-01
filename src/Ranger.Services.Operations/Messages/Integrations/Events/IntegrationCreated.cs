using System;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Integrations.Events
{
    [MessageNamespaceAttribute("integrations")]
    public class IntegrationCreated : IEvent
    {
        public string Domain { get; }
        public string Name { get; }
        public Guid Id { get; }

        public IntegrationCreated(string domain, string name, Guid id)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new System.ArgumentException($"{nameof(domain)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new System.ArgumentException($"{nameof(name)} was null or whitespace.");
            }

            this.Domain = domain;
            this.Name = name;
            this.Id = id;
        }
    }
}
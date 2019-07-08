using System;
using Newtonsoft.Json;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations {
    public class OperationPending : IEvent {
        public CorrelationContext CorrelationContext { get; }

        [JsonConstructor]
        public OperationPending (CorrelationContext correlationContext) {
            CorrelationContext = correlationContext;
        }
    }
}
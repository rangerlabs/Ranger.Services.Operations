using System;
using Newtonsoft.Json;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations {
    public class OperationCompleted : IEvent {
        public CorrelationContext CorrelationContext { get; set; }

        [JsonConstructor]
        public OperationCompleted (CorrelationContext correlationContext) {
            CorrelationContext = correlationContext;
        }
    }
}
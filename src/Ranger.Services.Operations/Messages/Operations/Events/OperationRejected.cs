using System;
using Newtonsoft.Json;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations {
    public class OperationRejected : IEvent {
        public CorrelationContext CorrelationContext { get; }
        public string Code { get; }
        public string Message { get; }

        [JsonConstructor]
        public OperationRejected (CorrelationContext correlationContext, string message, string code) {
            CorrelationContext = correlationContext;
            Code = code;
            Message = message;
        }
    }
}
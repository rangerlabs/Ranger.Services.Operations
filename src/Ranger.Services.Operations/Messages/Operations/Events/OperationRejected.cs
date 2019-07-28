using System;
using Newtonsoft.Json;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations {
    public class OperationRejected : IEvent {
        public string Code { get; }
        public string Message { get; }

        [JsonConstructor]
        public OperationRejected (string message, string code) {
            Code = code;
            Message = message;
        }
    }
}
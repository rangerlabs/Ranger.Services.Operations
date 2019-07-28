using System;
using Newtonsoft.Json;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations {
    public class OperationCompleted : IEvent {

        [JsonConstructor]
        public OperationCompleted () { }
    }
}
using System;
using Newtonsoft.Json;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations {
    public class OperationPending : IEvent {

        [JsonConstructor]
        public OperationPending () { }
    }
}
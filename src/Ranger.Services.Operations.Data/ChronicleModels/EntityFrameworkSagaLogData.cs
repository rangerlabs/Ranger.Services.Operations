using System;
using Chronicle;
using Newtonsoft.Json;

namespace Ranger.Services.Operations.Data
{
    public class EntityFrameworkSagaLogData : ISagaLogData
    {
        public SagaId SagaId { get; }
        public Type SagaType { get; }
        public long CreatedAt { get; }
        public object Message { get; }
        public Type MessageType { get; }

        [JsonConstructor]
        public EntityFrameworkSagaLogData(SagaId sagaId, Type sagaType, long createdAt, object message, Type messageType)
        {
            this.SagaId = sagaId;
            this.SagaType = sagaType;
            this.CreatedAt = createdAt;
            this.Message = message;
            this.MessageType = messageType;
        }

        public static ISagaLogData Create(SagaId sagaId, Type sagaType, object message)
                    => new EntityFrameworkSagaLogData(sagaId, sagaType, DateTimeOffset.Now.ToUnixTimeMilliseconds(), message, message.GetType());
    }
}
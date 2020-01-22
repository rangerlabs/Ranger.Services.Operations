using System;
using Chronicle;
using Newtonsoft.Json;

namespace Ranger.Services.Operations.Data
{
    public class EntityFrameworkSagaLogData : ISagaLogData
    {
        [JsonIgnore]
        public SagaId Id => SagaId;
        public string SagaId { get; set; }
        public string DatabaseUsername { get; set; }
        public Type Type { get; }
        public long CreatedAt { get; }
        public object Message { get; }
        public Type MessageType { get; }

        public EntityFrameworkSagaLogData(SagaId id, Type type, string databaseUsername, long createdAt, object message, Type messageType)
        {
            this.SagaId = id;
            this.DatabaseUsername = databaseUsername;
            this.Type = type;
            this.CreatedAt = createdAt;
            this.Message = message;
            this.MessageType = messageType;
        }
    }
}
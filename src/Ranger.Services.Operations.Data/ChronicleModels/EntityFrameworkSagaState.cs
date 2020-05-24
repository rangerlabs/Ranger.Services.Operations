using System;
using Chronicle;
using Newtonsoft.Json;

namespace Ranger.Services.Operations.Data
{
    public class EntityFrameworkSagaState : ISagaState
    {
        [JsonIgnore]
        public SagaId Id => SagaId;
        public string SagaId { get; set; }
        public string TenantId { get; set; }
        public Type Type { get; set; }
        public SagaStates State { get; set; }
        public object Data { get; set; }
        public Type DataType { get; set; }

        public EntityFrameworkSagaState(SagaId id, Type type, string tenantId, SagaStates state, object data, Type dataType)
        {
            this.SagaId = id;
            this.TenantId = tenantId;
            this.Type = type;
            this.State = state;
            this.Data = data;
            this.DataType = dataType;
        }

        public void Update(SagaStates state, object data = null)
        {
            State = state;
            Data = data;
        }
    }
}
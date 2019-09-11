using System;
using Chronicle;
using Newtonsoft.Json;

namespace Ranger.Services.Operations.Data
{
    public class EntityFrameworkSagaState : ISagaState
    {
        public SagaId SagaId { get; }

        public Type SagaType { get; }

        public SagaStates State { get; private set; }

        public object Data { get; private set; }

        public Type DataType { get; }

        [JsonConstructor]
        public EntityFrameworkSagaState(SagaId sagaId, Type sagaType, SagaStates state, object data = null, Type dataType = null)
            => (SagaId, SagaType, State, Data, DataType) = (sagaId, sagaType, state, data, dataType);

        public static ISagaState Create(SagaId sagaId, Type sagaType, SagaStates state, object data = null, Type dataType = null)
            => new EntityFrameworkSagaState(sagaId, sagaType, state, data, dataType);

        public void Update(SagaStates state, object data = null)
        {
            State = state;
            Data = data;
        }
    }
}
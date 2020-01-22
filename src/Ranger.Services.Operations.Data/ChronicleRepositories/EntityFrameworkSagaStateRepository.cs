using System;
using System.Linq;
using System.Threading.Tasks;
using Chronicle;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ranger.Services.Operations.Data
{
    public class EntityFrameworkSagaStateRepository : ISagaStateRepository
    {
        private readonly OperationsDbContext context;

        public EntityFrameworkSagaStateRepository(OperationsDbContext context)
        {
            this.context = context;
        }

        public async Task<ISagaState> ReadAsync(SagaId sagaId, Type sagaType)
        {
            if (String.IsNullOrWhiteSpace(sagaId))
            {
                throw new ArgumentNullException(nameof(sagaId));
            }
            if (sagaType is null)
            {
                throw new ArgumentNullException(nameof(sagaType));
            }

            EntityFrameworkSagaState state = null;
            var cachedSagaState = await context.SagaStates.FirstOrDefaultAsync(ss => ss.SagaId == sagaId && ss.SagaType == sagaType.ToString());
            if (cachedSagaState != null && !String.IsNullOrWhiteSpace(cachedSagaState.Data))
            {
                state = JsonConvert.DeserializeObject<EntityFrameworkSagaState>(cachedSagaState.Data);
                var data = (state.Data as JObject).ToObject(state.DataType);
                state.Update(state.State, data);
            }
            return state;
        }

        public async Task WriteAsync(ISagaState state)
        {
            if (state is EntityFrameworkSagaState efSagaState)
            {
                var entityFrameworkSagaState = new EntityFrameworkSagaState(efSagaState.Id, efSagaState.Type, efSagaState.DatabaseUsername, efSagaState.State, efSagaState?.Data, efSagaState?.Data.GetType());

                var serializedSagaState = JsonConvert.SerializeObject(entityFrameworkSagaState);
                var cachedSagaState = await context.SagaStates.FirstOrDefaultAsync(ss => ss.SagaId == efSagaState.SagaId && ss.SagaType == efSagaState.Type.ToString());
                if (cachedSagaState is null)
                {
                    var sagaState = new SagaState
                    {
                        SagaId = efSagaState.Id,
                        SagaType = efSagaState.Type.ToString(),
                        Data = serializedSagaState
                    };
                    context.SagaStates.Add(sagaState);
                }
                else
                {
                    cachedSagaState.Data = serializedSagaState;
                    context.SagaStates.Update(cachedSagaState);
                }
                await context.SaveChangesAsync();
            }
            else
            {
                throw new ArgumentNullException(nameof(state));
            }
        }

        public async Task DeleteAsync(SagaId sagaId, Type sagaType)
        {
            if (String.IsNullOrWhiteSpace(sagaId))
            {
                throw new ArgumentException(nameof(sagaId));
            }
            if (sagaType is null)
            {
                throw new ArgumentException(nameof(sagaType));
            }
            var sagaState = await context.SagaStates.FirstOrDefaultAsync(ss => ss.SagaId == sagaId && ss.SagaType == sagaType.ToString());
            if (sagaState != null)
            {
                context.SagaStates.Remove(sagaState);
                await context.SaveChangesAsync();
            }
        }
    }
}
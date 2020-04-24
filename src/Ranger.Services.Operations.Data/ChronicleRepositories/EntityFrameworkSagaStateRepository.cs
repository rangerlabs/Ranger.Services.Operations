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
            if (state is null)
            {
                throw new ArgumentNullException($"{nameof(state)} was null");
            }

            var entityFrameworkSagaState = new EntityFrameworkSagaState(state.Id, state.Type, (state.Data as BaseSagaData).TenantId, state.State, state.Data, state.Data.GetType());

            var serializedSagaState = JsonConvert.SerializeObject(entityFrameworkSagaState);
            var cachedSagaState = await context.SagaStates.FirstOrDefaultAsync(ss => ss.SagaId == state.Id && ss.SagaType == state.Type.ToString());
            if (cachedSagaState is null)
            {
                var sagaState = new SagaState
                {
                    SagaId = state.Id,
                    TenantId = (state.Data as BaseSagaData).TenantId,
                    SagaType = state.Type.ToString(),
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
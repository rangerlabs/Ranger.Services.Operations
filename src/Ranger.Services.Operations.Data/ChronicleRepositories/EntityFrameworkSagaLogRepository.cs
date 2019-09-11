using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chronicle;
using Chronicle.Persistence;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ranger.Services.Operations.Data;

namespace Ranger.Services.Operations.Data
{
    public class EntityFrameworkSagaLogRepository : ISagaLog
    {
        private readonly OperationsDbContext context;

        public EntityFrameworkSagaLogRepository(OperationsDbContext context)
        {
            this.context = context;
        }

        public async Task DeleteAsync(SagaId sagaId, Type sagaType)
        {
            var sagaLogDatas = await context.SagaLogDatas.Where(sld => sld.SagaId == sagaId && sld.SagaType == sagaType.ToString()).ToListAsync();
            if (sagaLogDatas.Count > 0)
            {
                context.SagaLogDatas.RemoveRange(sagaLogDatas);
                await context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ISagaLogData>> ReadAsync(SagaId id, Type sagaType)
        {
            if (String.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(nameof(id));
            }
            if (sagaType is null)
            {
                throw new ArgumentException(nameof(sagaType));
            }
            IList<EntityFrameworkSagaLogData> deserializedSagaLogDatas = new List<EntityFrameworkSagaLogData>();
            var sagaLogDataStrings = await context.SagaLogDatas.Where(sld => sld.SagaId == id && sld.SagaType == sagaType.ToString()).ToListAsync();
            sagaLogDataStrings.ForEach(sld =>
            {
                if (!String.IsNullOrWhiteSpace(sld.Data))
                {
                    var sagaLogData = JsonConvert.DeserializeObject<EntityFrameworkSagaLogData>(sld.Data);
                    var message = (sagaLogData.Message as JObject).ToObject(sagaLogData.MessageType);
                    deserializedSagaLogDatas.Add(new EntityFrameworkSagaLogData(sagaLogData.SagaId, sagaLogData.SagaType, sagaLogData.CreatedAt, message, message.GetType()));
                }
            });
            return deserializedSagaLogDatas;
        }

        public async Task WriteAsync(ISagaLogData sagaLogData)
        {
            if (sagaLogData is null)
            {
                throw new ArgumentException(nameof(sagaLogData));
            }

            var entityFrameworkSagaLogData = new EntityFrameworkSagaLogData(
                sagaLogData.SagaId,
                sagaLogData.SagaType,
                sagaLogData.CreatedAt,
                sagaLogData.Message,
                sagaLogData.Message.GetType()
                );

            var logData = new SagaLogData
            {
                SagaId = sagaLogData.SagaId,
                SagaType = sagaLogData.SagaType.ToString(),
                Data = JsonConvert.SerializeObject(entityFrameworkSagaLogData)
            };

            context.SagaLogDatas.Add(logData);
            await context.SaveChangesAsync();
        }
    }
}
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
            List<EntityFrameworkSagaLogData> sagaLogDatas = new List<EntityFrameworkSagaLogData>();
            IEnumerable<EntityFrameworkSagaLogData> serializedSagaLogDatas = new List<EntityFrameworkSagaLogData>();
            var sagaLogDataString = await context.SagaLogDatas.FirstOrDefaultAsync(sld => sld.SagaId == id && sld.SagaType == sagaType.ToString());
            if (sagaLogDataString != null && !String.IsNullOrWhiteSpace(sagaLogDataString.Data))
            {
                sagaLogDatas = JsonConvert.DeserializeObject<List<EntityFrameworkSagaLogData>>(sagaLogDataString.Data);
                serializedSagaLogDatas = sagaLogDatas.Select(s =>
                {
                    var message = (s.Message as JObject).ToObject(s.MessageType);
                    return new EntityFrameworkSagaLogData(s.SagaId, s.SagaType, s.CreatedAt, message, message.GetType());
                });
            }
            return serializedSagaLogDatas;
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
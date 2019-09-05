using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Chronicle.Persistence
{
    public class RedisEncryptedSagaLog : ISagaLog
    {
        private readonly IDistributedCache cache;
        private readonly IDataProtector dataProtector;

        public RedisEncryptedSagaLog(IDistributedCache cache, IDataProtectionProvider dataProtectionProvider)
        {
            this.cache = cache;
            this.dataProtector = dataProtectionProvider.CreateProtector(nameof(RedisEncryptedSagaLog));
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
            List<SagaLogData> sagaLogDatas = new List<SagaLogData>();
            IEnumerable<SagaLogData> serializedSagaLogDatas = new List<SagaLogData>();
            var cachedSagaLogDatasString = await cache.GetStringAsync(LogId(id, sagaType));
            if (!String.IsNullOrWhiteSpace(cachedSagaLogDatasString))
            {
                var unProtectedCachedSagaLogDatasString = dataProtector.Unprotect(cachedSagaLogDatasString);
                sagaLogDatas = JsonConvert.DeserializeObject<List<SagaLogData>>(unProtectedCachedSagaLogDatasString);
                serializedSagaLogDatas = sagaLogDatas.Select(s =>
                {
                    var message = (s.Message as JObject).ToObject(s.MessageType);
                    return new SagaLogData(s.SagaId, s.SagaType, s.CreatedAt, message, s.MessageType);
                });
            }
            return serializedSagaLogDatas;
        }

        public async Task WriteAsync(ISagaLogData logData)
        {
            if (logData is null)
            {
                throw new ArgumentException(nameof(logData));
            }
            IList<ISagaLogData> sagaLogDatas = (await ReadAsync(logData.SagaId, logData.SagaType)).ToList();
            sagaLogDatas.Add(logData);

            var sagaLogDatasString = JsonConvert.SerializeObject(sagaLogDatas);
            var protectedSagaLogDatasString = dataProtector.Protect(sagaLogDatasString);
            await cache.SetStringAsync(LogId(logData.SagaId, logData.SagaType), protectedSagaLogDatasString);

            await Task.CompletedTask;
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

            await cache.RemoveAsync(LogId(sagaId, sagaType));
            await Task.CompletedTask;
        }

        private string LogId(string id, Type type) => $"_log_{id}_{type.GetHashCode()}";
    }
}

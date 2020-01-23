using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chronicle;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Data
{

    public class OperationsRepository : IOperationsRepository
    {
        private readonly OperationsDbContext context;

        public OperationsRepository(OperationsDbContext context)
        {
            this.context = context;
        }

        public async Task<EntityFrameworkSagaStateResponse> GetSagaState(SagaId id, string databaseUsername)
        {
            EntityFrameworkSagaState state = null;
            var cachedSagaState = await context.SagaStates.FirstOrDefaultAsync(_ => _.SagaId == id && _.DatabaseUsername == databaseUsername);
            if (cachedSagaState != null && !String.IsNullOrWhiteSpace(cachedSagaState.Data))
            {
                state = JsonConvert.DeserializeObject<EntityFrameworkSagaState>(cachedSagaState.Data);
                var data = (state.Data as JObject).ToObject(state.DataType);
                state.Update(state.State, data);
            }

            var sagaLogs = await GetSagaLogs(id);

            var startTime = string.Empty;
            var endTime = string.Empty;
            if (state.State == SagaStates.Pending)
            {
                startTime = GetPendingSagaMetrics(sagaLogs);
            }
            else
            {
                (startTime, endTime) = GetFinishedSagaMetrics(sagaLogs);
            }

            var result = new EntityFrameworkSagaStateResponse
            {
                Id = state.Id,
                State = Enum.GetName(typeof(SagaStates), state.State),
                StartTime = startTime,
                EndTime = endTime,
                Inititor = (state.Data is BaseSagaData bsd) ? bsd.Initiator : string.Empty
            };
            return result;
        }

        private string GetRejectedReason(IEnumerable<ISagaLogData> sagaLogs)
        {
            var orderedSagaLogs = sagaLogs.OrderByDescending(_ => _.CreatedAt);
            var lastMessage = orderedSagaLogs.FirstOrDefault();
            return lastMessage.Type is IRejectedEvent re ? re.Reason : "A reason could not be provided for the rejection of this request.";
        }

        private string GetPendingSagaMetrics(IEnumerable<ISagaLogData> sagaLogs)
        {
            var orderedSagaLogs = sagaLogs.OrderByDescending(_ => _.CreatedAt);
            return DateTimeOffset.FromUnixTimeMilliseconds(orderedSagaLogs.Last().CreatedAt).ToUniversalTime().ToString("R");
        }

        private (string StartTime, string EndTime) GetFinishedSagaMetrics(IEnumerable<ISagaLogData> sagaLogs)
        {
            var orderedSagaLogs = sagaLogs.OrderByDescending(_ => _.CreatedAt);
            var startDate = DateTimeOffset.FromUnixTimeMilliseconds(orderedSagaLogs.Last().CreatedAt).ToUniversalTime().ToString("R");
            var endDateTime = DateTimeOffset.FromUnixTimeMilliseconds(orderedSagaLogs.First().CreatedAt).ToUniversalTime().ToString("R");
            return (startDate, endDateTime);
        }

        private async Task<IEnumerable<ISagaLogData>> GetSagaLogs(SagaId id)
        {
            IList<EntityFrameworkSagaLogData> deserializedSagaLogDatas = new List<EntityFrameworkSagaLogData>();
            var sagaLogDataStrings = await context.SagaLogDatas.Where(_ => _.SagaId == id).ToListAsync();
            sagaLogDataStrings.ForEach(sld =>
            {
                if (!String.IsNullOrWhiteSpace(sld.Data))
                {

                    var sagaLogData = JsonConvert.DeserializeObject<EntityFrameworkSagaLogData>(sld.Data);
                    var message = (sagaLogData.Message as JObject).ToObject(sagaLogData.MessageType);
                    deserializedSagaLogDatas.Add(new EntityFrameworkSagaLogData(sagaLogData.Id, sagaLogData.Type, sagaLogData.CreatedAt, message, message.GetType()));
                }
            });
            return deserializedSagaLogDatas;
        }
    }
}
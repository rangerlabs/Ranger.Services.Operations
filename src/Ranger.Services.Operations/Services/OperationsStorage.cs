using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Ranger.Services.Operations {
    public class OperationsStorage : IOperationsStorage {
        private readonly IDistributedCache _cache;

        public OperationsStorage (IDistributedCache cache) {
            _cache = cache;
        }

        public async Task SetAsync (Guid id, Guid userId, string name, OperationStateEnum state,
            string resource, string code = null, string reason = null) {
            var newState = state.ToString ().ToLowerInvariant ();
            var operation = await GetAsync (id);
            operation = operation ?? new OperationDto ();
            operation.Id = id;
            operation.UserId = userId;
            operation.Name = name;
            operation.State = newState;
            operation.Resource = resource;
            operation.Code = code ?? string.Empty;
            operation.Reason = reason ?? string.Empty;

            await _cache.SetStringAsync (id.ToString ("N"),
                JsonConvert.SerializeObject (operation),
                new DistributedCacheEntryOptions {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes (10),
                        SlidingExpiration = TimeSpan.FromMinutes (1)
                });
        }

        public async Task<OperationDto> GetAsync (Guid id) {
            var operation = await _cache.GetStringAsync (id.ToString ("N"));

            return string.IsNullOrWhiteSpace (operation) ? null : JsonConvert.DeserializeObject<OperationDto> (operation);
        }
    }
}
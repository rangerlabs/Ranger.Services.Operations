using System;
using System.Threading.Tasks;
using Ranger.Services.Operations.Data;

namespace Ranger.Services.Operations
{
    public interface IOperationsStorage
    {
        Task<OperationDto> GetAsync(Guid id);

        Task SetAsync(Guid id, Guid userEmail, string name, OperationsStateEnum state,
            string resource, string code = null, string reason = null);
    }
}
using System.Threading.Tasks;
using Chronicle;

namespace Ranger.Services.Operations.Data
{
    public interface IOperationsRepository
    {
        Task<EntityFrameworkSagaStateResponse> GetSagaState(SagaId id, string tenantId);
    }
}
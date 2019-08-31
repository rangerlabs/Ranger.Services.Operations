using System.Threading.Tasks;
using Chronicle;
using Ranger.Services.Operations.Data;

namespace Ranger.Services.Operations
{
    public interface IOperationsPublisher
    {
        Task SendRangerLabsStatusUpdate(ISagaContext context, string domain, OperationsStateEnum state);
    }
}
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Projects
{
    [MessageNamespace("projects")]
    internal class InitializeTenant : ICommand
    {
        public string TenantId { get; }
        public string DatabasePassword { get; }

        public InitializeTenant(string tenantId, string databasePassword)
        {
            this.TenantId = tenantId;
            this.DatabasePassword = databasePassword;
        }
    }
}
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Identity
{
    [MessageNamespace("identity")]
    public class DropTenant : ICommand
    {
        public string DatabaseUsername { get; }

        public DropTenant(string databaseUsername)
        {
            this.DatabaseUsername = databaseUsername;
        }
    }
}
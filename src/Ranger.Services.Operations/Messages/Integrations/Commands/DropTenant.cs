using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Integrations
{
    [MessageNamespace("integrations")]
    public class DropTenant : ICommand
    {
        public string DatabaseUsername { get; }

        public DropTenant(string databaseUsername)
        {
            this.DatabaseUsername = databaseUsername;
        }
    }
}
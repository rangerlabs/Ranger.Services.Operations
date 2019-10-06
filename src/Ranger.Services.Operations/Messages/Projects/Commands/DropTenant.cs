using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Projects
{
    [MessageNamespace("projects")]
    public class DropTenant : ICommand
    {
        public string DatabaseUsername { get; }

        public DropTenant(string databaseUsername)
        {
            this.DatabaseUsername = databaseUsername;
        }
    }
}
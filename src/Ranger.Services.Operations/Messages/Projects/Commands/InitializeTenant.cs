using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Projects
{
    [MessageNamespace("projects")]
    internal class InitializeTenant : ICommand
    {
        public string DatabaseUsername { get; }
        public string DatabasePassword { get; }

        public InitializeTenant(string databaseUsername, string databasePassword)
        {
            this.DatabaseUsername = databaseUsername;
            this.DatabasePassword = databasePassword;
        }
    }
}
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Identity
{
    [MessageNamespace("identity")]
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
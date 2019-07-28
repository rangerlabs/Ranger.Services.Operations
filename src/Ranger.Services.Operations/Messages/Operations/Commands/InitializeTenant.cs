using Ranger.RabbitMQ;

namespace Ranger.Services.Operations {
    [MessageNamespace ("operations")]
    internal class InitializeTenant : ICommand {
        public string DatabaseUsername { get; }
        public string DatabasePassword { get; }

        public InitializeTenant (string databaseUsername, string databasePassword) {
            this.DatabaseUsername = databaseUsername;
            this.DatabasePassword = databasePassword;
        }
    }
}
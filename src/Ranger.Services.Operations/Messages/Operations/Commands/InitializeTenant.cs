using Ranger.RabbitMQ;

namespace Ranger.Services.Operations {
    [MessageNamespace ("operations")]
    internal class InitializeTenant : ICommand {
        public CorrelationContext CorrelationContext { get; }
        public string DatabaseUsername { get; }
        public string DatabasePassword { get; }

        public InitializeTenant (CorrelationContext correlationContext, string databaseUsername, string databasePassword) {
            this.CorrelationContext = correlationContext;
            this.DatabaseUsername = databaseUsername;
            this.DatabasePassword = databasePassword;
        }
    }
}
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Operations
{
    [MessageNamespaceAttribute("operations")]
    public class DeleteGeofenceSagaInitializer : SagaInitializer, ICommand
    {
        public DeleteGeofenceSagaInitializer(bool frontendRequest, string commandingUserEmailOrTokenPrefix, string domain, string externalId, string projectId)
        {
            if (string.IsNullOrWhiteSpace(commandingUserEmailOrTokenPrefix))
            {
                throw new System.ArgumentException($"{nameof(commandingUserEmailOrTokenPrefix)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new System.ArgumentException($"{nameof(domain)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(externalId))
            {
                throw new System.ArgumentException($"{nameof(externalId)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(projectId))
            {
                throw new System.ArgumentException($"{nameof(projectId)} was null or whitespace.");
            }

            this.FrontendRequest = frontendRequest;
            this.CommandingUserEmailOrTokenPrefix = commandingUserEmailOrTokenPrefix;

            Domain = domain;
            this.ExternalId = externalId;
            this.ProjectId = projectId;
        }
        public bool FrontendRequest { get; }
        public string CommandingUserEmailOrTokenPrefix { get; }
        public string ExternalId { get; }
        public string ProjectId { get; }
    }
}
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Geofences
{
    [MessageNamespaceAttribute("geofences")]
    public class GeofenceCreated : IEvent
    {
        public string Domain { get; }
        public string ExternalId { get; }
        public string Id { get; }

        public GeofenceCreated(string domain, string externalId, string id)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new System.ArgumentException($"{nameof(domain)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(externalId))
            {
                throw new System.ArgumentException($"{nameof(externalId)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new System.ArgumentException($"{nameof(id)} was null or whitespace.");
            }

            this.Domain = domain;
            this.ExternalId = externalId;
            this.Id = id;
        }
    }
}
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Geofences.Events
{
    [MessageNamespace("geofences")]
    public class IntegrationPurgedFromGeofences : IEvent
    { }
}
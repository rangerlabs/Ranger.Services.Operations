using Ranger.RabbitMQ;

namespace Ranger.Services.Operations
{
    [MessageNamespace("operations")]
    public class PrimaryOwnershipTransferAccepted : IEvent
    {
        public PrimaryOwnershipTransferAccepted(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new System.ArgumentException($"{nameof(token)} was null or whitespace.");
            }

            this.Token = token;

        }
        public string Token { get; private set; }
    }
}
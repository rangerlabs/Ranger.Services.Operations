using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Notifications
{
    [MessageNamespace("notifications")]
    public class SendPrimaryOwnerTransferEmails : ICommand
    {
        public string TransferEmail { get; }
        public string OwnerEmail { get; }
        public string TransferFirstName { get; }
        public string OwnerFirstName { get; }
        public string OwnerLastName { get; }
        public string Domain { get; }
        public string OrganizationName { get; }
        public string Token { get; }
        public string CorrelationId { get; }

        public SendPrimaryOwnerTransferEmails(string transferEmail, string ownerEmail, string transferFirstName, string ownerFirstName, string ownerLastName, string domain, string organizationName, string token, string correlationId)
        {
            if (string.IsNullOrWhiteSpace(transferEmail))
            {
                throw new System.ArgumentException(nameof(transferEmail));
            }
            if (string.IsNullOrWhiteSpace(ownerEmail))
            {
                throw new System.ArgumentException(nameof(ownerEmail));
            }
            if (string.IsNullOrWhiteSpace(transferFirstName))
            {
                throw new System.ArgumentNullException(nameof(transferFirstName));
            }
            if (string.IsNullOrWhiteSpace(ownerFirstName))
            {
                throw new System.ArgumentNullException(nameof(ownerFirstName));
            }
            if (string.IsNullOrWhiteSpace(ownerLastName))
            {
                throw new System.ArgumentNullException(nameof(ownerLastName));
            }
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new System.ArgumentNullException(nameof(domain));
            }
            if (string.IsNullOrWhiteSpace(organizationName))
            {
                throw new System.ArgumentNullException(nameof(organizationName));
            }
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new System.ArgumentNullException(nameof(token));
            }
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                throw new System.ArgumentNullException(nameof(correlationId));
            }

            this.OwnerEmail = ownerEmail;
            this.TransferEmail = transferEmail;
            this.TransferFirstName = transferFirstName;
            this.OwnerFirstName = ownerFirstName;
            this.OwnerLastName = ownerLastName;
            this.Domain = domain;
            this.OrganizationName = organizationName;
            this.Token = token;
            this.CorrelationId = correlationId;
        }
    }
}
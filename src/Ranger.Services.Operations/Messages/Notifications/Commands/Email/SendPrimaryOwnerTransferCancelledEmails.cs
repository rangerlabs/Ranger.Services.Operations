using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Notifications
{
    [MessageNamespace("notifications")]
    public class SendPrimaryOwnerTransferCancelledEmails : ICommand
    {
        public string TransferEmail { get; }
        public string OwnerEmail { get; }
        public string TransferFirstName { get; }
        public string OwnerFirstName { get; }
        public string OwnerLastName { get; }
        public string TenantId { get; }

        public SendPrimaryOwnerTransferCancelledEmails(string transferEmail, string ownerEmail, string transferFirstName, string ownerFirstName, string ownerLastName, string tenantId)
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
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new System.ArgumentNullException(nameof(tenantId));
            }
            this.TransferEmail = transferEmail;
            this.OwnerEmail = ownerEmail;
            this.TransferFirstName = transferFirstName;
            this.OwnerFirstName = ownerFirstName;
            this.OwnerLastName = ownerLastName;
            this.TenantId = tenantId;
        }
    }
}
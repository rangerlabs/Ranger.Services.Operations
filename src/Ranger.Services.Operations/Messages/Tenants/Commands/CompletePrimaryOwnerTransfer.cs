using System;
using Ranger.Common;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations.Messages.Tenants.Commands
{
    [MessageNamespace("tenants")]
    public class CompletePrimaryOwnerTransfer : ICommand
    {
        public CompletePrimaryOwnerTransfer(string domain, string commandingUserEmail, PrimaryOwnerTransferStateEnum state)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(commandingUserEmail))
            {
                throw new ArgumentException($"{nameof(commandingUserEmail)} was null or whitespace.");
            }
            if (state is PrimaryOwnerTransferStateEnum.Pending)
            {
                throw new ArgumentException("'Pending' is an invalid status to complete a Primary Owner Transfer.");
            }

            this.Domain = domain;
            this.CommandingUserEmail = commandingUserEmail;
            this.State = state;
        }
        public string Domain { get; }
        public string CommandingUserEmail { get; }
        public PrimaryOwnerTransferStateEnum State { get; }
    }
}
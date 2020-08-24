using Ranger.RabbitMQ;
using Ranger.Services.Operations.Messages.Operations;

namespace Ranger.Services.Operations
{
    [MessageNamespaceAttribute("operations")]
    public class DeleteUserSagaInitializer : SagaInitializer, ICommand
    {
        public DeleteUserSagaInitializer(string tenantId, string email, string commandingUserEmail)
        {
            if (string.IsNullOrEmpty(tenantId))
            {
                throw new System.ArgumentException($"{nameof(tenantId)} was null or whitespace");
            }

            if (string.IsNullOrEmpty(email))
            {
                throw new System.ArgumentException($"{nameof(email)} was null or whitespace");
            }

            if (string.IsNullOrEmpty(commandingUserEmail))
            {
                throw new System.ArgumentException($"{nameof(commandingUserEmail)} was null or whitespace");
            }

            TenantId = tenantId;
            this.Email = email;
            this.CommandingUserEmail = commandingUserEmail;

        }
        public string Email { get; }
        public string CommandingUserEmail { get; }


    }
}
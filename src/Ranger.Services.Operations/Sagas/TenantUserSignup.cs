using System.Threading.Tasks;
using Chronicle;
using Microsoft.Extensions.Logging;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations {
    public class TenantUserSignup : Saga,
        ISagaStartAction<TenantCreated>,
        ISagaAction<UserCreated> {
            private readonly IBusPublisher busPublisher;
            private readonly ILogger<TenantUserSignup> logger;

            public TenantUserSignup (IBusPublisher busPublisher, ILogger<TenantUserSignup> logger) {
                this.busPublisher = busPublisher;
                this.logger = logger;
            }
            public async Task CompensateAsync (TenantCreated message, ISagaContext context) {
                logger.LogInformation ("Performing TenantCreated Compensate.");
            }

            public async Task CompensateAsync (UserCreated message, ISagaContext context) {
                logger.LogInformation ("Performing UserCreated Compensate.");
            }

            public async Task HandleAsync (TenantCreated message, ISagaContext context) {
                this.busPublisher.SendAsync (new CreateUser ("", "", "", "", "", "", "", message.CorrelationContext));
            }

            public async Task HandleAsync (UserCreated message, ISagaContext context) {
                await CompleteAsync ();
            }
        }
}
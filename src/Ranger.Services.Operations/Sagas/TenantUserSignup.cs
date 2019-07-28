using System;
using System.Threading.Tasks;
using Chronicle;
using Microsoft.Extensions.Logging;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations {
    public class TenantUserSignup : Saga<UserData>,
        ISagaStartAction<TenantCreated>,
        ISagaAction<IdentityTenantInitialized>,
        ISagaAction<IdentityInitializeTenantRejected>,
        ISagaAction<UserCreated> {
            private readonly IBusPublisher busPublisher;
            private readonly ILogger<TenantUserSignup> logger;

            public TenantUserSignup (IBusPublisher busPublisher, ILogger<TenantUserSignup> logger) {
                this.busPublisher = busPublisher;
                this.logger = logger;
            }

            public async Task HandleAsync (TenantCreated message, ISagaContext context) {
                await Task.Run (() => {
                    Data.User = message.User;
                    Data.Domain = message.DomainName;
                    this.busPublisher.Send (new InitializeTenant (message.DatabaseUsername, message.DatabasePassword), CorrelationContext.FromId (Guid.Parse (context.SagaId)));
                });
            }

            public async Task HandleAsync (IdentityTenantInitialized message, ISagaContext context) {
                await Task.Run (() =>
                    this.busPublisher.Send (
                        new CreateUser (
                            Data.User.Email,
                            Data.User.FirstName,
                            Data.User.LastName,
                            Data.User.Password,
                            Data.Domain,
                            "Owner",
                            ""
                        ),
                        CorrelationContext.FromId (Guid.Parse (context.SagaId))
                    )
                );
            }

            public async Task CompensateAsync (IdentityTenantInitialized message, ISagaContext context) {
                await RejectAsync ();
            }

            public async Task HandleAsync (IdentityInitializeTenantRejected message, ISagaContext context) {
                await Task.CompletedTask;
            }

            public async Task CompensateAsync (IdentityInitializeTenantRejected message, ISagaContext context) {
                await Task.CompletedTask;
            }

            public async Task CompensateAsync (TenantCreated message, ISagaContext context) {
                await Task.Run (() => logger.LogInformation ("Performing TenantCreated Compensate."));
            }

            public async Task HandleAsync (UserCreated message, ISagaContext context) {
                logger.LogInformation ("TenantUserSignup saga completed succesfully.");
                await CompleteAsync ();
            }

            public async Task CompensateAsync (UserCreated message, ISagaContext context) {
                await Task.Run (() => logger.LogInformation ("Performing UserCreated Compensate."));
            }
        }

    public class UserData {
        public User User { get; set; }
        public string Domain { get; set; }
    }
}
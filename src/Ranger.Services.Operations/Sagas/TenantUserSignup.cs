using System;
using System.Threading.Tasks;
using Chronicle;
using Microsoft.Extensions.Logging;
using PusherServer;
using Ranger.RabbitMQ;

namespace Ranger.Services.Operations {
    public class TenantUserSignup : Saga<UserData>,
        ISagaStartAction<TenantCreated>,
        ISagaAction<IdentityTenantInitialized>,
        ISagaAction<IdentityInitializeTenantRejected>,
        ISagaAction<NewTenantOwnerCreated> {
            private readonly IBusPublisher busPublisher;
            private readonly ILogger<TenantUserSignup> logger;

            public TenantUserSignup (IBusPublisher busPublisher, ILogger<TenantUserSignup> logger) {
                this.busPublisher = busPublisher;
                this.logger = logger;
            }

            public async Task HandleAsync (TenantCreated message, ISagaContext context) {
                await Task.Run (() => {
                    Data.Owner = message.Owner;
                    Data.Domain = message.DomainName;
                    this.busPublisher.Send (new InitializeTenant (message.DatabaseUsername, message.DatabasePassword), CorrelationContext.FromId (Guid.Parse (context.SagaId)));
                });
            }

            public async Task HandleAsync (IdentityTenantInitialized message, ISagaContext context) {
                await Task.Run (() =>
                    this.busPublisher.Send (
                        new CreateNewTenantOwner (
                            Data.Owner.Email,
                            Data.Owner.FirstName,
                            Data.Owner.LastName,
                            Data.Owner.Password,
                            Data.Domain,
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

            public async Task HandleAsync (NewTenantOwnerCreated message, ISagaContext context) {
                var options = new PusherOptions {
                    Cluster = "us2",
                    Encrypted = true
                };

                var pusher = new Pusher (
                    "828034",
                    "aed7ba7c7247aca9680e",
                    "df532af7ccf602593aa5",
                    options);

                var result = await pusher.TriggerAsync (
                    "ranger-labs",
                    "registration-event",
                    new { domain = Data.Domain, correlationId = context.SagaId.Id, status = OperationState.Completed }
                );

                logger.LogInformation ("TenantUserSignup saga completed succesfully.");
                await CompleteAsync ();
            }

            public async Task CompensateAsync (NewTenantOwnerCreated message, ISagaContext context) {
                await Task.Run (() => logger.LogInformation ("Performing NewTenantOwnerCreated Compensate."));
            }
        }

    public class UserData {
        public NewTenantOwner Owner { get; set; }
        public string Domain { get; set; }
    }
}
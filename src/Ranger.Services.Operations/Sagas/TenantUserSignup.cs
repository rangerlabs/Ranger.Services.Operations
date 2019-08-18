using System;
using System.Collections.Generic;
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
        ISagaAction<GeofencesTenantInitialized>,
        ISagaAction<GeofencesInitializeTenantRejected>,
        ISagaAction<NewTenantOwnerCreated>,
        ISagaAction<SendNewTenantOwnerEmailSent> {
            const int SERVICES_TO_BE_INITIALIZED = 2;
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

            //start action - will never be called
            public Task CompensateAsync (TenantCreated message, ISagaContext context) {
                throw new NotImplementedException ();
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
                await RejectAsync ();
            }

            public async Task HandleAsync (NewTenantOwnerCreated message, ISagaContext context) {
                await Task.Run (() => {
                    if (AllServicesInitialized ("identity")) {
                        SendNewTenantOwnerEmail (context);
                    }
                });
            }

            public async Task CompensateAsync (NewTenantOwnerCreated message, ISagaContext context) {
                await Task.Run (() => logger.LogInformation ("Performing NewTenantOwnerCreated Compensate."));
                await RejectAsync ();
            }

            public async Task HandleAsync (SendNewTenantOwnerEmailSent message, ISagaContext context) {
                await SendPusherStatusUpdate (context);

                logger.LogInformation ("TenantUserSignup saga completed succesfully.");
                await CompleteAsync ();
            }

            public async Task CompensateAsync (SendNewTenantOwnerEmailSent message, ISagaContext context) {
                await RejectAsync ();
            }

            public async Task HandleAsync (GeofencesInitializeTenantRejected message, ISagaContext context) {
                await Task.CompletedTask;
            }

            public async Task CompensateAsync (GeofencesInitializeTenantRejected message, ISagaContext context) {
                await RejectAsync ();
            }

            public async Task HandleAsync (GeofencesTenantInitialized message, ISagaContext context) {
                await Task.Run (() => {
                    if (AllServicesInitialized ("geofences")) {
                        SendNewTenantOwnerEmail (context);
                    }
                });
            }

            public async Task CompensateAsync (GeofencesTenantInitialized message, ISagaContext context) {
                await RejectAsync ();
            }

            private bool AllServicesInitialized (string service) {
                if (Data.ServicesInitialized.Count == SERVICES_TO_BE_INITIALIZED - 1) {
                    return true;
                }
                Data.ServicesInitialized.Add (service);
                return false;
            }
            private void SendNewTenantOwnerEmail (ISagaContext context) {
                busPublisher.Send (new SendNewTenantOwnerEmail (Data.Owner.Email, Data.Owner.FirstName, Data.Domain, ""),
                    CorrelationContext.FromId (Guid.Parse (context.SagaId))
                );
            }

            private async Task SendPusherStatusUpdate (ISagaContext context) {
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
                    new { domain = Data.Domain, correlationId = context.SagaId.Id, status = OperationStateEnum.Completed }
                );
            }
        }

    public class UserData {
        public NewTenantOwner Owner { get; set; }
        public string Domain { get; set; }
        public List<string> ServicesInitialized { get; set; } = new List<string> ();
    }
}
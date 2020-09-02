using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chronicle;
using Microsoft.Extensions.Logging;
using Ranger.RabbitMQ;
using Ranger.RabbitMQ.BusPublisher;
using Ranger.Services.Operations.Messages.Geofences;
using Ranger.Services.Operations.Messages.Integrations.Commands;
using Ranger.Services.Operations.Messages.Projects.Events;
using Ranger.Services.Operations.Messages.Subscriptions.Commands;
using Ranger.Services.Subscriptions;
using Ranger.Services.Subscriptions.Messages.Events;

namespace Ranger.Services.Operations.Sagas
{
    public class EnforceSubscriptionsSaga : Saga<EnforceSubscriptionsData>,
        ISagaStartAction<EnforceSubscriptions>,
        ISagaAction<TenantLimitDetailsComputed>,
        ISagaAction<ProjectResourceLimitsEnforced>
    {
        private readonly IBusPublisher busPublisher;
        private readonly ILogger<EnforceSubscriptionsSaga> logger;

        public EnforceSubscriptionsSaga(IBusPublisher busPublisher, ILogger<EnforceSubscriptionsSaga> logger)
        {
            this.busPublisher = busPublisher;
            this.logger = logger;
        }

        public Task CompensateAsync(EnforceSubscriptions message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(TenantLimitDetailsComputed message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task CompensateAsync(ProjectResourceLimitsEnforced message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            return Task.CompletedTask;
        }

        public Task HandleAsync(EnforceSubscriptions message, ISagaContext context)
        {
            logger.LogDebug($"Calling handle for message '{message.GetType()}'");
            Data.TenantId = Constants.SystemTaskTenantId;
            Data.Initiator = "SubscriptionEnforcer";
            busPublisher.Send(new ComputeTenantLimitDetails(message.TenantIds), CorrelationContext.FromId(Guid.Parse(context.SagaId)));
            return Task.CompletedTask;
        }

        public Task HandleAsync(TenantLimitDetailsComputed message, ISagaContext context)
        {
            logger.LogDebug($"Calling compensate for message '{message.GetType()}'");
            Data.TenantLimitDetails = message.TenantLimitDetails;
            busPublisher.Send(
                new EnforceProjectResourceLimits(Data.TenantLimitDetails.Select(tl => (tl.Item1, tl.Item2.Projects))),
                CorrelationContext.FromId(Guid.Parse(context.SagaId))
            );
            return Task.CompletedTask;
        }

        public async Task HandleAsync(ProjectResourceLimitsEnforced message, ISagaContext context)
        {
            busPublisher.Send(
                    new EnforceGeofenceResourceLimits(Data.TenantLimitDetails.Select(tl =>
                        (
                            tl.tenantId,
                            tl.limitFields.Geofences,
                            message.TenantRemainingProjects.Where(_ => _.tenantId == tl.tenantId).Single().remainingProjectIds
                        ))),
                    CorrelationContext.FromId(Guid.Parse(context.SagaId))
            );
            busPublisher.Send(
                    new EnforceIntegrationResourceLimits(Data.TenantLimitDetails.Select(tl =>
                        (
                            tl.tenantId,
                            tl.limitFields.Integrations,
                            message.TenantRemainingProjects.Where(_ => _.tenantId == tl.tenantId).Single().remainingProjectIds
                        ))),
                    CorrelationContext.FromId(Guid.Parse(context.SagaId))
            );
            await CompleteAsync();
        }
    }

    public class EnforceSubscriptionsData : BaseSagaData
    {
        public IEnumerable<(string tenantId, LimitFields limitFields)> TenantLimitDetails { get; set; }
    }
}
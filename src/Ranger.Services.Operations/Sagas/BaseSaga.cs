using System;
using System.Threading.Tasks;
using Chronicle;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.Services.Operations.Messages.Operations;

namespace Ranger.Services.Operations
{
    public abstract class BaseSaga<TSaga, TData> : Saga<TData>
        where TSaga : class
        where TData : class, new()
    {
        private readonly ITenantsClient tenantsClient;
        private readonly ILogger<TSaga> logger;

        protected BaseSaga(ITenantsClient tenantsClient, ILogger<TSaga> logger)
        {
            this.tenantsClient = tenantsClient;
            this.logger = logger;
        }

        public async Task<string> GetPgsqlDatabaseUsernameOrReject(SagaInitializer message)
        {
            ContextTenant tenant = null;
            try
            {
                tenant = await this.tenantsClient.GetTenantAsync<ContextTenant>(message.Domain);
            }
            catch (HttpClientException<ContextTenant> ex)
            {
                if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status404NotFound)
                {
                    logger.LogError($"A saga was initiated for domain '{message.Domain}' that could not be found. Rejecting Saga.");
                    await RejectAsync();
                }
            }
            catch (Exception)
            {
                logger.LogError($"Failed to retrieve the tenant for domain '{message.Domain}' that could not be found. Rejecting Saga.");
                await RejectAsync();
            }
            return tenant?.DatabaseUsername;
        }
    }
}
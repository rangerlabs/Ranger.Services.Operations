using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.Services.Operations.Data;

namespace Ranger.Services.Operations.Controllers
{
    [ApiController]
    public class OperationsController : ControllerBase
    {
        private readonly IOperationsRepository sagaStateRepository;
        private readonly ITenantsClient tenantsClient;
        private readonly ILogger<OperationsController> logger;

        public OperationsController(IOperationsRepository sagaStateRepository, ITenantsClient tenantsClient, ILogger<OperationsController> logger)
        {
            this.sagaStateRepository = sagaStateRepository;
            this.tenantsClient = tenantsClient;
            this.logger = logger;
        }

        [HttpGet("/{domain}/operations/{id}")]
        public async Task<IActionResult> GetAllGeofences([FromRoute] string domain, [FromRoute] Guid id)
        {
            ContextTenant tenant = null;
            try
            {
                tenant = await this.tenantsClient.GetTenantAsync<ContextTenant>(domain);
            }
            catch (HttpClientException ex)
            {
                if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status404NotFound)
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "An exception occurred retrieving the ContextTenant object.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            try
            {
                var result = await this.sagaStateRepository.GetSagaState(id.ToString(), tenant.DatabaseUsername);
                return Ok(result);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"An exception occurred retrieving the saga state for domain '{domain}' and saga id '{id}'");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
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
        private readonly TenantsHttpClient tenantsClient;
        private readonly ILogger<OperationsController> logger;

        public OperationsController(IOperationsRepository sagaStateRepository, TenantsHttpClient tenantsClient, ILogger<OperationsController> logger)
        {
            this.sagaStateRepository = sagaStateRepository;
            this.tenantsClient = tenantsClient;
            this.logger = logger;
        }
    }
}
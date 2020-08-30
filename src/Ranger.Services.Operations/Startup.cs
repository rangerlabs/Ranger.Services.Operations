using System;
using System.Security.Cryptography.X509Certificates;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Chronicle;
using Chronicle.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using Ranger.ApiUtilities;
using Ranger.InternalHttpClient;
using Ranger.Monitoring.HealthChecks;
using Ranger.RabbitMQ;
using Ranger.Services.Operations.Data;

namespace Ranger.Services.Operations
{
    public class Startup
    {
        private readonly IWebHostEnvironment Environment;
        private readonly IConfiguration configuration;
        private ILoggerFactory loggerFactory;

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            this.Environment = environment;
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers(options =>
                 {
                     options.EnableEndpointRouting = false;
                 })
                 .AddNewtonsoftJson(options =>
                 {
                     options.SerializerSettings.Converters.Add(new StringEnumConverter());
                     options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                     options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                     options.SerializerSettings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
                 });

            services.AddRangerApiVersioning();
            services.ConfigureAutoWrapperModelStateResponseFactory();
            services.AddSwaggerGen("Operations API", "v1");

            services.AddAuthorization(options =>
            {
                options.AddPolicy("operationsApi", policyBuilder =>
                {
                    policyBuilder.RequireScope("operationsApi");
                });
            });

            var identityAuthority = configuration["httpClient:identityAuthority"];
            services.AddPollyPolicyRegistry();
            services.AddTenantsHttpClient("http://tenants:8082", identityAuthority, "tenantsApi", "cKprgh9wYKWcsm");
            services.AddProjectsHttpClient("http://projects:8086", identityAuthority, "projectsApi", "usGwT8Qsp4La2");
            services.AddIdentityHttpClient("http://identity:5000", identityAuthority, "IdentityServerApi", "89pCcXHuDYTXY");

            services.AddDbContext<OperationsDbContext>(options =>
            {
                options.UseNpgsql(configuration["cloudSql:ConnectionString"]);
            },
                ServiceLifetime.Transient
            );

            services.AddTransient<IOperationsDbContextInitializer, OperationsDbContextInitializer>();
            services.AddTransient<IOperationsRepository, OperationsRepository>();

            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "http://identity:5000/auth";
                    options.ApiName = "operationsApi";
                    options.RequireHttpsMetadata = false;
                });

            services.AddDataProtection()
                .SetApplicationName("Operations")
                .ProtectKeysWithCertificate(new X509Certificate2(configuration["DataProtectionCertPath:Path"]))
                .UnprotectKeysWithAnyCertificate(new X509Certificate2(configuration["DataProtectionCertPath:Path"]))
                .PersistKeysToDbContext<OperationsDbContext>();

            services.AddChronicle(b =>
            {
                b.UseSagaLog<EntityFrameworkSagaLogRepository>();
                b.UseSagaStateRepository<EntityFrameworkSagaStateRepository>();
            });

            services.AddLiveHealthCheck();
            services.AddEntityFrameworkHealthCheck<OperationsDbContext>();
            services.AddDockerImageTagHealthCheck();
            services.AddRabbitMQHealthCheck();

        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.AddRabbitMq();
            builder.RegisterGeneric(typeof(GenericEventHandler<>))
                .As(typeof(IMessageHandler<>));
            builder.RegisterGeneric(typeof(GenericCommandHandler<>))
                .As(typeof(IMessageHandler<>));
        }

        public void Configure(IApplicationBuilder app, IHostApplicationLifetime applicationLifetime)
        {
            app.UseSwagger("v1", "Operations API");
            app.UseAutoWrapper();
            app.UseUnhandedExceptionLogger();
            app.UseRouting();
            app.UseAuthentication();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks();
                endpoints.MapLiveTagHealthCheck();
                endpoints.MapEfCoreTagHealthCheck();
                endpoints.MapDockerImageTagHealthCheck();
                endpoints.MapRabbitMQHealthCheck();
            });

            app.UseRabbitMQ().SubscribeAllMessages();
        }
    }
}
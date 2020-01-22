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
using Newtonsoft.Json.Serialization;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;
using Ranger.Services.Operations.Data;

namespace Ranger.Services.Operations
{
    public class Startup
    {
        private readonly IWebHostEnvironment Environment;
        private readonly IConfiguration configuration;
        private ILoggerFactory loggerFactory;
        private IBusSubscriber busSubscriber;

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
                     options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                     options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                 });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("operationsApi", policyBuilder =>
                {
                    policyBuilder.RequireScope("operationsApi");
                });
            });


            services.AddSingleton<IProjectsClient, ProjectsClient>(provider =>
            {
                return new ProjectsClient("http://projects:8086", loggerFactory.CreateLogger<ProjectsClient>());
            });
            services.AddSingleton<ITenantsClient, TenantsClient>(provider =>
            {
                return new TenantsClient("http://tenants:8082", loggerFactory.CreateLogger<TenantsClient>());
            });

            services.AddEntityFrameworkNpgsql().AddDbContext<OperationsDbContext>(options =>
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

                    //TODO: Change these to true
                    options.RequireHttpsMetadata = false;
                });

            services.AddDataProtection()
                .ProtectKeysWithCertificate(new X509Certificate2(configuration["DataProtectionCertPath:Path"]))
                .PersistKeysToDbContext<OperationsDbContext>();

            services.AddChronicle(b =>
            {
                b.UseSagaLog<EntityFrameworkSagaLogRepository>();
                b.UseSagaStateRepository<EntityFrameworkSagaStateRepository>();
            });

        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.AddRabbitMq();
            builder.RegisterGeneric(typeof(GenericEventHandler<>))
                .As(typeof(IMessageHandler<>));
            builder.RegisterGeneric(typeof(GenericCommandHandler<>))
                .As(typeof(IMessageHandler<>));
        }

        public void Configure(IApplicationBuilder app, IHostApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;

            applicationLifetime.ApplicationStopping.Register(OnShutdown);

            app.UseRouting();
            app.UseAuthentication();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            this.busSubscriber = app.UseRabbitMQ()
                .SubscribeAllMessages();
        }

        private void OnShutdown()
        {
            this.busSubscriber.Dispose();
        }
    }
}
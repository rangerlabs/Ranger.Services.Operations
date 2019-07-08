using System;
using System.Security.Cryptography.X509Certificates;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Chronicle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Ranger.RabbitMQ;
using Ranger.Redis;
using Ranger.Services.Operations.Data;

namespace Ranger.Services.Operations {
    public class Startup {
        private readonly IConfiguration configuration;
        private readonly ILogger<Startup> logger;
        private IContainer container;
        private IBusSubscriber busSubscriber;

        public Startup (IConfiguration configuration, ILogger<Startup> logger) {
            this.configuration = configuration;
            this.logger = logger;
        }

        public IServiceProvider ConfigureServices (IServiceCollection services) {
            services.AddMvcCore (options => {
                    var policy = ScopePolicy.Create ("operationScope");
                    options.Filters.Add (new AuthorizeFilter (policy));
                })
                .AddAuthorization ()
                .AddJsonFormatters ()
                .AddJsonOptions (options => {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver ();
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });

            services.AddEntityFrameworkNpgsql ().AddDbContext<OperationsDbContext> (options => {
                    options.UseNpgsql (configuration["CloudSql:OperationsConnectionString"]);
                },
                ServiceLifetime.Transient
            );

            services.AddTransient<IOperationsDbContextInitializer, OperationsDbContextInitializer> ();

            services.AddAuthentication ("Bearer")
                .AddIdentityServerAuthentication (options => {
                    options.Authority = "http://identity:5000/auth";
                    options.ApiName = "operationsApi";

                    //TODO: Change these to true
                    options.EnableCaching = false;
                    options.RequireHttpsMetadata = false;
                });

            if (Environment.GetEnvironmentVariable ("ASPNETCORE_ENVIRONMENT") == EnvironmentName.Production) {
                services.AddDataProtection ()
                    .ProtectKeysWithCertificate (new X509Certificate2 (configuration["DataProtectionCertPath:Path"]))
                    .PersistKeysToDbContext<OperationsDbContext> ();
                this.logger.LogInformation ("Production data protection certificate loaded.");
            } else {
                services.AddDataProtection ();
            }

            services.AddRedis ();
            services.AddChronicle ();

            var builder = new ContainerBuilder ();
            builder.Populate (services);
            builder.AddRabbitMq ();
            builder.RegisterGeneric (typeof (GenericEventHandler<>))
                .As (typeof (IMessageHandler<>));
            builder.RegisterGeneric (typeof (GenericCommandHandler<>))
                .As (typeof (IMessageHandler<>));
            container = builder.Build ();
            return new AutofacServiceProvider (container);
        }

        public void Configure (IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime applicationLifetime) {
            applicationLifetime.ApplicationStopping.Register (OnShutdown);
            app.UseAuthentication ();
            app.UseMvcWithDefaultRoute ();
            this.busSubscriber = app.UseRabbitMQ ()
                .SubscribeAllMessages ();
        }

        private void OnShutdown () {
            this.busSubscriber.Dispose ();
        }
    }
}
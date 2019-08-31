﻿using System;
using System.Security.Cryptography.X509Certificates;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Chronicle;
using Chronicle.Persistence;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PusherServer;
using Ranger.Common;
using Ranger.RabbitMQ;
using Ranger.Redis;
using Ranger.Services.Operations.Data;

namespace Ranger.Services.Operations
{
    public class Startup
    {
        private readonly IConfiguration configuration;
        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger<Startup> logger;
        private IContainer container;
        private IBusSubscriber busSubscriber;

        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory, ILogger<Startup> logger)
        {
            this.configuration = configuration;
            this.loggerFactory = loggerFactory;
            this.logger = logger;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore(options =>
            {
                var policy = ScopePolicy.Create("operationsScope");
                options.Filters.Add(new AuthorizeFilter(policy));
            })
                .AddAuthorization()
                .AddJsonFormatters()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });

            services.AddEntityFrameworkNpgsql().AddDbContext<OperationsDbContext>(options =>
            {
                options.UseNpgsql(configuration["cloudSql:ConnectionString"]);
            },
                ServiceLifetime.Transient
            );

            services.AddTransient<IOperationsDbContextInitializer, OperationsDbContextInitializer>();

            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "http://identity:5000/auth";
                    options.ApiName = "operationsApi";

                    //TODO: Change these to true
                    options.EnableCaching = false;
                    options.RequireHttpsMetadata = false;
                });

            services.AddDataProtection()
                .ProtectKeysWithCertificate(new X509Certificate2(configuration["DataProtectionCertPath:Path"]))
                .PersistKeysToDbContext<OperationsDbContext>();

            services.AddOptions<RangerPusherOptions>("pusher");
            services.AddSingleton<IPusher>(s =>
            {
                var options = s.GetService<RangerPusherOptions>();
                return new Pusher(options.AppId, options.Key, options.Secret, new PusherOptions { Cluster = options.Cluster, Encrypted = bool.Parse(options.Encrypted) });
            });
            services.AddSingleton<IOperationsPublisher, OperationsPublisher>();
            services.AddRedis(logger);
            services.AddChronicle(b =>
            {
                b.UseRedisPersistence();
                b.DeleteOnCompleted();
            });

            var builder = new ContainerBuilder();
            builder.Populate(services);
            builder.AddRabbitMq(loggerFactory);
            builder.RegisterGeneric(typeof(GenericEventHandler<>))
                .As(typeof(IMessageHandler<>));
            builder.RegisterGeneric(typeof(GenericCommandHandler<>))
                .As(typeof(IMessageHandler<>));
            container = builder.Build();
            return new AutofacServiceProvider(container);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime applicationLifetime)
        {
            applicationLifetime.ApplicationStopping.Register(OnShutdown);
            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();
            this.busSubscriber = app.UseRabbitMQ()
                .SubscribeAllMessages();
        }

        private void OnShutdown()
        {
            this.busSubscriber.Dispose();
        }
    }
}
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Palantir.Identity.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Palantir.Identity.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Palantir.Identity.Services;
using Palantir.Identity.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using System.Linq;
using System.Collections.Generic;
using MailKit;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
//using Palantir.EventBus.EventBusRabbitMQ;
using Microsoft.Extensions.Logging;
//using RabbitMQ.Client;
//using Palantir.EventBus.Abstractions;
//using Palantir.Identity.IntegrationEvents;
//using Palantir.EventBus;
using Palantir.Identity.AutofacModules;
using System;

namespace Palantir.Identity
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            //services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
            //{
            //    var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();


            //    var factory = new ConnectionFactory()
            //    {
            //        HostName = Configuration["EventBusConnection"],
            //        DispatchConsumersAsync = true,
            //        AutomaticRecoveryEnabled = true,
            //        ClientProvidedName = "Palantir.Identity"
            //    };

            //    if (!string.IsNullOrEmpty(Configuration["EventBusUserName"]))
            //    {
            //        factory.UserName = Configuration["EventBusUserName"];
            //    }

            //    if (!string.IsNullOrEmpty(Configuration["EventBusPassword"]))
            //    {
            //        factory.Password = Configuration["EventBusPassword"];
            //    }

            //    var retryCount = 5;
            //    if (!string.IsNullOrEmpty(Configuration["EventBusRetryCount"]))
            //    {
            //        retryCount = int.Parse(Configuration["EventBusRetryCount"]);
            //    }

            //    return new DefaultRabbitMQPersistentConnection(factory, logger, retryCount);
            //});

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    Configuration.GetConnectionString("Postgres")));
            services.AddIdentity<PalantirUser, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddUserManager<PalantirUserManager>()
        .AddRoleManager<PalantirRoleManager>()
        .AddDefaultTokenProviders();
            services.Configure<EmailRelay>(Configuration.GetSection("Relay"));

            services.AddTransient<IEmailSender, EmailService>();
            //services.AddTransient<ISmsSender, AuthMessageSender>();

            // Adds IdentityServer
            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApiResources())
                .AddInMemoryClients(Config.GetClients())
                .AddAspNetIdentity<PalantirUser>();

            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            }).ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var keys = context.ModelState.Keys.ToList();
                    var values = context.ModelState.Values.ToList();
                    var response = new Dictionary<string, string[]>();
                    for (var i = 0; i < context.ModelState.ErrorCount; i++)
                    {
                        var key = keys[i];
                        var errors = values[i].Errors.Select(e => e.ErrorMessage).ToArray();
                        response.Add(key.ToLowerInvariant(), errors);
                    }
                    return new UnprocessableEntityObjectResult(response);
                };
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Unspecified;
                options.OnAppendCookie = context => context.CookieOptions.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                options.OnDeleteCookie = context => context.CookieOptions.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Identity.API", Version = "v1" });
            });
            
            //RegisterEventBus(services);
            //configure autofac
            var container = new ContainerBuilder();
            container.RegisterModule(new ApplicationModule());
            container.Populate(services);

            return new AutofacServiceProvider(container.Build());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCookiePolicy();
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseIdentityServer();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
            app.UseSwagger().UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/v1/swagger.json", "Identity.API V1");
                c.RoutePrefix = "sw";
            });
            //ConfigureEventBus(app);
        }

        //private void ConfigureEventBus(IApplicationBuilder app)
        //{
        //    var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();

        //    eventBus.Subscribe<NotificationIntegrationEvent, NotificationIntegrationEventHandler>();
        //}

        //private void RegisterEventBus(IServiceCollection services)
        //{
        //    var subscriptionClientName = Configuration["SubscriptionClientName"];


        //    services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
        //    {
        //        var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
        //        var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
        //        var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
        //        var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

        //        var retryCount = 5;
        //        if (!string.IsNullOrEmpty(Configuration["EventBusRetryCount"]))
        //        {
        //            retryCount = int.Parse(Configuration["EventBusRetryCount"]);
        //        }

        //        return new EventBusRabbitMQ(rabbitMQPersistentConnection, logger, iLifetimeScope, eventBusSubcriptionsManager, subscriptionClientName, retryCount);
        //    });


        //    services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();
        //}
    }
}

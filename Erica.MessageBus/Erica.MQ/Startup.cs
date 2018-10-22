﻿using Erica.MQ.Models.SQL;
using Erica.MQ.Services.SQL;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Erica.MQ.Services.DotNetOverrides;
using Erica.MQ.Services.SignalrHubs;
using SharedInterfaces.Interfaces.DataTransferObjects;
using Erica.MQ.Interfaces.Factory;
using Erica.MQ.Services.Factory;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.SignalR;
using System.Text;
using SharedInterfaces.Constants.IdentityServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Erica.MessageBus
{
    public class Startup
    {  
        private IConfiguration _configuration { get; set; }
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();
            _configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {

            var connection = _configuration.GetConnectionString("EricaMqDBConnection");
            services.AddDbContext<EricaMQ_DBContext>(options => options.UseSqlServer(connection));
            services.AddTransient<IEricaMQ_MessageDTO, EricaMQ_Message>();
            services.AddTransient<IConsumerAdapterFactory, ConsumerAdapterFactory>(); 
            
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddMvc()
                   .AddJsonOptions(o =>
                   {
                       o.SerializerSettings.ContractResolver.ResolveContract(typeof(IEricaMQ_MessageDTO)).Converter = new MyJsonConverter<IEricaMQ_MessageDTO, EricaMQ_Message>();
                       o.SerializerSettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto;
                   });

            services.AddMvcCore()
                .AddAuthorization()
                .AddJsonFormatters();

            services.AddAuthentication(
                    options =>
                    {
                        options.DefaultAuthenticateScheme = Constants_IdentityServer.Bearer;
                        options.DefaultChallengeScheme = Constants_IdentityServer.Bearer;
                    }
                )
                .AddIdentityServerAuthentication(
                    options =>
                    {
                        options.Authority = Constants_IdentityServer.IdentityServerUrl;
                        options.RequireHttpsMetadata = false;
                        options.ApiName = Constants_IdentityServer.EricaMQ_Api;
                    }
                );  

            services.AddSignalR();
             
            // Change to use Name as the user identifier for SignalR
            // WARNING: This requires that the source of your JWT token 
            // ensures that the Name claim is unique!
            // If the Name claim isn't unique, users could receive messages 
            // intended for a different user!
            services.AddSingleton<IUserIdProvider, NameUserIdProvider>();
        } 
       
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            loggerFactory.AddLog4Net("log4net.config");
            app.UseAuthentication();           
            app.UseMvc(); 
            app.UseSignalR(
                routes => routes.MapHub<EricaMQ_Hub>("/api/ericamqhub")
                );           
        }
    }
}

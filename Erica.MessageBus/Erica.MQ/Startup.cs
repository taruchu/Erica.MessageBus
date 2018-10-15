using Erica.MQ.Models.SQL;
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
using IdentityServer.IdentityServerConstants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;

namespace Erica.MessageBus
{
    public class Startup
    {  
        public static readonly SymmetricSecurityKey EricaMQ_SecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("9DD48C9E-3A87-4C6A-8CB6-3A85296F94A7"));

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var connection = @"Server=JESUS;Database=EricaMQ;Trusted_Connection=True;";
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
                        options.DefaultAuthenticateScheme = Constants.Bearer;
                        options.DefaultChallengeScheme = Constants.Bearer;
                    }
                )
                .AddIdentityServerAuthentication(
                    options =>
                    {
                        options.Authority = Constants.IdentityServerUrl;
                        options.RequireHttpsMetadata = false;
                        options.ApiName = Constants.EricaMQ_Api;
                    }
                );
                //.AddJwtBearer(options =>
                //{
                     
                //    // We have to hook the OnMessageReceived event in order to
                //    // allow the JWT authentication handler to read the access
                //    // token from the query string when a WebSocket or 
                //    // Server-Sent Events request comes in.
                //    options.Events = new JwtBearerEvents
                //    {
                //        OnMessageReceived = context =>
                //        {
                //            var accessToken = context.Request.Query["access_token"];

                //            // If the request is for our hub...
                //            var path = context.HttpContext.Request.Path;
                //            if (!string.IsNullOrEmpty(accessToken) &&
                //                (path.StartsWithSegments("/api/ericamqhub")))
                //            {
                //                // Read the token out of the query string
                //                context.Token = accessToken;
                //            }
                //            return Task.CompletedTask;
                //        }
                //    };
                //});



            services.AddSignalR();
             
            // Change to use Name as the user identifier for SignalR
            // WARNING: This requires that the source of your JWT token 
            // ensures that the Name claim is unique!
            // If the Name claim isn't unique, users could receive messages 
            // intended for a different user!
            services.AddSingleton<IUserIdProvider, NameUserIdProvider>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();           
            app.UseMvc(); 
            app.UseSignalR(
                routes => routes.MapHub<EricaMQ_Hub>("/api/ericamqhub")
                );
           
        }
    }
}

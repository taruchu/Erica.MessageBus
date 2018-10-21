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
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.SignalR;
using System.Text;
using SharedInterfaces.Constants.IdentityServer;

namespace Erica.MessageBus
{
    public class Startup
    {  
        public static readonly SymmetricSecurityKey EricaMQ_SecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("9DD48C9E-3A87-4C6A-8CB6-3A85296F94A7"));
         
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

            services.AddSignalR();
             
            // Change to use Name as the user identifier for SignalR
            // WARNING: This requires that the source of your JWT token 
            // ensures that the Name claim is unique!
            // If the Name claim isn't unique, users could receive messages 
            // intended for a different user!
            services.AddSingleton<IUserIdProvider, NameUserIdProvider>();
        } 
       
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

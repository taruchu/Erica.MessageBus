using Erica.MQ.Producer.Services.DotNetOverrides;
using EricaChats.DataAccess.Models;
using EricaChats.DataAccess.Services.SQL;
using EricaChats.ProducerAdapter.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharedInterfaces.Constants.IdentityServer;
using SharedInterfaces.Interfaces.EricaChats;

namespace Erica.MQ.Producer
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
            var connection = _configuration.GetConnectionString("EricaChatsDBConnection");
            services.AddDbContext<EricaChats_DBContext>(options => options.UseSqlServer(connection));
            services.AddTransient<IEricaChats_MessageDTO, EricaChats_MessageDTO>();
            services.AddTransient<IEricaChatsSimpleProducerAdapter, EricaChatsSimpleProducerAdapter>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddMvc()
                .AddJsonOptions(o =>
                {
                    o.SerializerSettings.ContractResolver.ResolveContract(typeof(IEricaChats_MessageDTO)).Converter = new MyJsonConverter<IEricaChats_MessageDTO, EricaChats_MessageDTO>();
                    o.SerializerSettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto;
                });

            services.AddMvcCore()
                .AddAuthorization()
                .AddJsonFormatters();

            services.AddAuthentication(Constants_IdentityServer.Bearer)
                .AddIdentityServerAuthentication(
                    options =>
                    {
                        options.Authority = Constants_IdentityServer.IdentityServerUrl;
                        options.RequireHttpsMetadata = false;
                        options.ApiName = Constants_IdentityServer.EricaMQProducer_Api;
                    }
                );

            services.AddHttpClient();
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
        }
    }
}

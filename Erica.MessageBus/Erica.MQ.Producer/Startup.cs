using Erica.MQ.Producer.Services.DotNetOverrides;
using EricaChats.DataAccess.Models;
using EricaChats.DataAccess.Services.SQL;
using EricaChats.ProducerAdapter.Services;
using IdentityServer.IdentityServerConstants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedInterfaces.Interfaces.EricaChats;

namespace Erica.MQ.Producer
{
    public class Startup
    { 
        public void ConfigureServices(IServiceCollection services)
        {
            var connection = @"Server=JESUS;Database=EricaChats;Trusted_Connection=True;";
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

            services.AddAuthentication(Constants.Bearer)
                .AddIdentityServerAuthentication(
                    options =>
                    {
                        options.Authority = Constants.IdentityServerUrl;
                        options.RequireHttpsMetadata = false;
                        options.ApiName = Constants.EricaMQProducer_Api;
                    }
                );

            services.AddHttpClient();
        }
         
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}

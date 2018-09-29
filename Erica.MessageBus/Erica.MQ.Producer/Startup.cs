using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Erica.MQ.Producer.Models;
using Erica.MQ.Producer.Services.SQL;
using Erica.MQ.Producer.Services.DotNetOverrides;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedInterfaces.Interfaces.EricaChats;

namespace Erica.MQ.Producer
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var connection = @"Server=JESUS;Database=EricaChats;Trusted_Connection=True;";
            services.AddDbContext<EricaChats_DBContext>(options => options.UseSqlServer(connection));
            services.AddTransient<IEricaChats_MessageDTO, EricaChats_MessageDTO>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddMvc()
                .AddJsonOptions(o =>
                {
                    o.SerializerSettings.ContractResolver.ResolveContract(typeof(IEricaChats_MessageDTO)).Converter = new MyJsonConverter<IEricaChats_MessageDTO, EricaChats_MessageDTO>();
                    o.SerializerSettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto;
                });           

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
            
        }
    }
}

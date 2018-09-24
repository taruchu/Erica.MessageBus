﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Erica.MQ.Interfaces.DataTransferObjects;
using Erica.MQ.Interfaces.SQL;
using Erica.MQ.Models.SQL;
using Erica.MQ.Services.SQL;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Erica.MQ.Services.DotNetOverrides;
using Erica.MQ.Services.SignalrHubs;

namespace Erica.MessageBus
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var connection = @"Server=JESUS;Database=EricaMQ;Trusted_Connection=True;";
            services.AddDbContext<EricaMQ_DBContext>(options => options.UseSqlServer(connection));
            services.AddTransient<IEricaMQ_MessageDTO, EricaMQ_Message>();
            
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddMvc()
           .AddJsonOptions(o => {
               o.SerializerSettings.ContractResolver.ResolveContract(typeof(IEricaMQ_MessageDTO)).Converter = new MyJsonConverter<IEricaMQ_MessageDTO, EricaMQ_Message>();
               o.SerializerSettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto;
           });

           services.AddSignalRCore();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
            app.UseSignalR(
                routes => routes.MapHub<EricaMQ_Hub>("api/ericamqhub/getnewmessages")
                );
           
        }
    }
}

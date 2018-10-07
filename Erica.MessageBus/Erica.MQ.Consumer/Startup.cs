using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Erica.MQ.Consumer.Services.SignalrHubs;
using EricaChats.ConsumerAdapter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SharedInterfaces.Interfaces.EricaChats;

namespace Erica.MQ.Consumer
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IEricaChatsSimpleConsumerAdapter, EricaChatsSimpleConsumerAdapter>();
            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSignalR(
                routes => routes.MapHub<EricaConsumer_Hub>("/api/ericachatshub/getnewmessages")
                ); 
        }
    }
}

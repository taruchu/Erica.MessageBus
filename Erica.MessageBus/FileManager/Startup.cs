using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EricaChats.DataAccess.Services.SQL;
using FileManager.Interfaces.NoSql;
using FileManager.Models.Configuration;
using FileManager.Services.NoSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedInterfaces.DotNetOverrides;
using SharedInterfaces.Interfaces.EricaChats;
using SharedInterfaces.Models.DTO;

namespace FileManager
{
    public class Startup
    {
        private IConfiguration _configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(env.ContentRootPath)
               .AddJsonFile("appsettings.json")
               .AddEnvironmentVariables();
            _configuration = builder.Build(); 
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<Settings>(options =>
            {
                options.ConnectionString
                    = _configuration.GetSection("MongoConnection:ConnectionString").Value;
                options.Database
                    = _configuration.GetSection("MongoConnection:Database").Value;
            });
            var connection = _configuration.GetConnectionString("EricaChatsDBConnection");
            services.AddDbContext<EricaChats_DBContext>(options => options.UseSqlServer(connection));
            services.AddTransient<IEricaChatsFilesRepository, EricaChatsFilesDBContext>();
            services.AddTransient<IEricaChats_FileDTO, EricaChats_FileDTO>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddMvc()
                  .AddJsonOptions(o =>
                  {
                      o.SerializerSettings.ContractResolver.ResolveContract(typeof(IEricaChats_FileDTO)).Converter = new MyJsonConverter<IEricaChats_FileDTO, EricaChats_FileDTO>();
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
            else
            {
                app.UseHsts();
            }

            app.UseMvc();
        }
    }
}

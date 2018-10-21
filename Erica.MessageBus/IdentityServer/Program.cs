using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using SharedInterfaces.Constants.IdentityServer;

namespace IdentityServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls(Constants_IdentityServer.IdentityServerUrl);
    }
}

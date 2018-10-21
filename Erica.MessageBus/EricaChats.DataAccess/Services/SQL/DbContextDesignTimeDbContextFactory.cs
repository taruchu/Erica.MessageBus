using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace EricaChats.DataAccess.Services.SQL
{
    //NOTE: This class is for running migrations when the dbContext is not apart of the startup project.
    public class DbContextDesignTimeDbContextFactory : IDesignTimeDbContextFactory<EricaChats_DBContext>
    {
        public EricaChats_DBContext CreateDbContext(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();

            var connection = builder.Build().GetConnectionString("EricaChatsDBConnection");
            DbContextOptionsBuilder<EricaChats_DBContext> dbContextOptionsBuilder = new DbContextOptionsBuilder<EricaChats_DBContext>();
            dbContextOptionsBuilder.UseSqlServer<EricaChats_DBContext>(connection);
            return new EricaChats_DBContext(dbContextOptionsBuilder.Options);
        }
    }
}

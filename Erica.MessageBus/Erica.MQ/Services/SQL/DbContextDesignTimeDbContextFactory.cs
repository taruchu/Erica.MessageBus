using Erica.MQ.Services.Configuration;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Erica.MQ.Services.SQL
{
    public class DbContextDesignTimeDbContextFactory : IDesignTimeDbContextFactory<EricaMQ_DBContext>
    {
        public EricaMQ_DBContext CreateDbContext(string[] args)
        {
            SQLDBConfigurationProvider configurationProvider = new SQLDBConfigurationProvider();
            return new EricaMQ_DBContext(configurationProvider);
        }
    }
}

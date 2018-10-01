using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace EricaChats.DataAccess.Services.SQL
{
    public class DbContextDesignTimeDbContextFactory : IDesignTimeDbContextFactory<EricaChats_DBContext>
    {
        public EricaChats_DBContext CreateDbContext(string[] args)
        {
            var connection = @"Server=JESUS;Database=EricaChats;Trusted_Connection=True;";
            DbContextOptionsBuilder<EricaChats_DBContext> dbContextOptionsBuilder = new DbContextOptionsBuilder<EricaChats_DBContext>();
            dbContextOptionsBuilder.UseSqlServer<EricaChats_DBContext>(connection);
            return new EricaChats_DBContext(dbContextOptionsBuilder.Options);
        }
    }
}

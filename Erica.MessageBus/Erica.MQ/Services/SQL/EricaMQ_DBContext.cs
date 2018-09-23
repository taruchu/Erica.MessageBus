using Erica.MQ.Interfaces.DataTransferObjects;
using Erica.MQ.Interfaces.SQL;
using Erica.MQ.Models.SQL;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Erica.MQ.Services.SQL
{
    public class EricaMQ_DBContext : DbContext, IEricaMQ
    { 
        public DbSet<EricaMQ_Message> EricaMQ_Messages { get; set; } 
        public EricaMQ_DBContext(DbContextOptions<EricaMQ_DBContext> options) :base(options)
        {  
        }
          
        public string POST(IEricaMQ_MessageDTO jsonMessage)
        {
            throw new NotImplementedException();
        }

        public string PUT(IEricaMQ_MessageDTO jsonMessage)
        {
            throw new NotImplementedException();
        }

        public List<IEricaMQ_MessageDTO> GET(DateTime afterThisTimeStamp, int maxAmount)
        {
            throw new NotImplementedException();
        }
    }
}

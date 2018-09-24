﻿using Erica.MQ.Interfaces.DataTransferObjects;
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
            var optionss = options;
        }
          
        public IEricaMQ_MessageDTO POST(IEricaMQ_MessageDTO message)
        {
            using (var dbContextTransaction = this.Database.BeginTransaction())
            {
                try
                { 
                    message.CreatedDateTime = DateTime.Now;
                    message.ModifiedDateTime = DateTime.Now;
                    message.Id = 0;
                    this.EricaMQ_Messages.Add((EricaMQ_Message)message); //NOTE: Use cast to tranform DTO into model
                    this.SaveChanges();
                    dbContextTransaction.Commit();
                    return message;
                }
                catch(Exception ex)
                {
                    dbContextTransaction.Rollback();
                    throw new ApplicationException(ex.Message, ex);
                }
            }
        }

        public IEricaMQ_MessageDTO PUT(IEricaMQ_MessageDTO message)
        {
            using (var dbContextTransaction = this.Database.BeginTransaction())
            {
                try
                {
                    EricaMQ_Message existingMessage = this.EricaMQ_Messages.Find(message.Id);

                    existingMessage.ModifiedDateTime = DateTime.Now;
                    existingMessage.Sender = message.Sender;
                    existingMessage.Data = message.Data;
                    existingMessage.Context = message.Context;
                    this.EricaMQ_Messages.Update(existingMessage);
                    this.SaveChanges();
                    dbContextTransaction.Commit();
                    return existingMessage;
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    throw new ApplicationException(ex.Message, ex);
                }
            }
        }

        public List<IEricaMQ_MessageDTO> GET(DateTime afterThisTimeStamp, int maxAmount)
        {
            throw new NotImplementedException();
        }
    }
}
using Erica.MQ.Interfaces.SQL;
using Erica.MQ.Models.SQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedInterfaces.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Erica.MQ.Services.SQL
{
    public class EricaMQ_DBContext : DbContext, IEricaMQ
    {
        public DbSet<EricaMQ_Message> EricaMQ_Messages { get; set; }
        private static ILogger _logger { get; set; }
        public EricaMQ_DBContext(DbContextOptions<EricaMQ_DBContext> options, ILoggerFactory loggerFactory) : base(options)
        {
            _logger = loggerFactory.CreateLogger(Assembly.GetExecutingAssembly().FullName);
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
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    _logger.LogError(ex, ex.Message);
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
                    existingMessage.AdapterAssemblyQualifiedName = message.AdapterAssemblyQualifiedName;
                    this.EricaMQ_Messages.Update(existingMessage);
                    this.SaveChanges();
                    dbContextTransaction.Commit();
                    return existingMessage;
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    _logger.LogError(ex, ex.Message);
                    throw new ApplicationException(ex.Message, ex);
                }
            }
        }

        public List<IEricaMQ_MessageDTO> GET(DateTime afterThisTimeStamp, int maxAmount, DateTime beforeThisTimeStamp)
        {
            using (var dbContextTransaction = this.Database.BeginTransaction())
            {
                try
                {
                    var newMessages = this.EricaMQ_Messages
                        .Where(msg => DateTime.Compare(msg.CreatedDateTime, afterThisTimeStamp) > 0 && DateTime.Compare(msg.CreatedDateTime, beforeThisTimeStamp) < 0)
                        .Take(maxAmount)
                        .ToList<IEricaMQ_MessageDTO>();
                    dbContextTransaction.Commit();
                    return newMessages;
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    _logger.LogError(ex, ex.Message);
                    throw new ApplicationException(ex.Message, ex);
                }
            }
        }

        public List<IEricaMQ_MessageDTO> GetByContext(DateTime afterThisTimeStamp, int maxAmount, DateTime beforeThisTimeStamp, string context)
        {
            return GET(afterThisTimeStamp, maxAmount, beforeThisTimeStamp)
                    .Where(dto => dto.Context == context)
                    .ToList<IEricaMQ_MessageDTO>();
        }

        public IEricaMQ_MessageDTO GetLatestMessge() 
        {
            using (var dbContextTransaction = this.Database.BeginTransaction())
            {
                try
                {
                    var latestMessage = this.EricaMQ_Messages 
                        .OrderByDescending(msg => msg.CreatedDateTime)
                        .FirstOrDefault();
                    dbContextTransaction.Commit();
                    return latestMessage;
                }
                catch(Exception ex)
                {
                    dbContextTransaction.Rollback();
                    _logger.LogError(ex, ex.Message);
                    throw new ApplicationException(ex.Message, ex);
                }
            }
        }
    }
}

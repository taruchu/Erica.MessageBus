using Erica.MQ.Interfaces.Factory;
using Erica.MQ.Services.SQL;
using EricaChats.DataAccess.Models;
using EricaMQ.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SharedInterfaces.Interfaces.DataTransferObjects;
using SharedInterfaces.Interfaces.EricaChats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Erica.MQ.Services.SignalrHubs
{
    [Authorize]
    public class EricaMQ_Hub : Hub
    {
        private EricaMQ_DBContext _ericaMQ_DBContext { get; set; } 
        private IConsumerAdapterFactory _consumerAdapterFactory { get; set; }

        public static string GroupNameLatestMessage = "latestMessageGroup"; 

        public EricaMQ_Hub(EricaMQ_DBContext ericaMQ_DBContext, IConsumerAdapterFactory consumerAdapterFactory)
        {
            _ericaMQ_DBContext = ericaMQ_DBContext;
            _consumerAdapterFactory = consumerAdapterFactory; 
        } 

        private List<IEricaMQ_MessageDTO> GetMessageListInRange(DateTime afterThisTimeStamp, int maxAmount, DateTime beforeThisTimeStamp, string context = null)
        {
            var newMessages = (context == null) ? _ericaMQ_DBContext.GET(afterThisTimeStamp, maxAmount, beforeThisTimeStamp)
                            : _ericaMQ_DBContext.GetByContext(afterThisTimeStamp, maxAmount, beforeThisTimeStamp, context);
            return newMessages;
        } 
        
        public async Task<bool> GetMessagesInRangeBulkList(DateTime afterThisTimeStamp, int maxAmount, DateTime beforeThisTimeStamp)
        {
            try
            {
                var newMessages = GetMessageListInRange(afterThisTimeStamp, maxAmount, beforeThisTimeStamp);
                List<string> bulkList = new List<string>();
                int percentComplete = 0;
                foreach(var message in newMessages)
                {
                    string marshalledMessage = JsonMarshaller.Marshall(message);
                    bulkList.Add(marshalledMessage);
                    percentComplete = (bulkList.Count / 100) * newMessages.Count;
                    await Clients.Caller.SendAsync("ReceiveMessagesInRangeBulkListProcessingProgress", percentComplete);
                }

                await Clients.Caller.SendAsync("ReceiveMessagesInRangeBulkList", bulkList);
                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message, ex);
            }
        }

        public async Task<bool> ConsumeMessagesInRangeBulkList(DateTime afterThisTimeStamp, int maxAmount, DateTime beforeThisTimeStamp)
        {
            try
            {
                var newMessages = GetMessageListInRange(afterThisTimeStamp, maxAmount, beforeThisTimeStamp);
                List<string> bulkList = new List<string>();
                int percentComplete = 0;
                int iterationCount = 0; 
                foreach (var message in newMessages)
                {
                    iterationCount++;
                    //NOTE: Messages must have an Adapter type defined in order to be consumed.
                    if (String.IsNullOrEmpty(message.AdapterAssemblyQualifiedName) == false)
                    {
                        string consumedMessage = _consumerAdapterFactory.Consume(message);
                        bulkList.Add(consumedMessage);
                        percentComplete = (iterationCount / 100) * newMessages.Count; //NOTE: If any messages are skipped, bulkList count will never equal newMessages count
                        await Clients.Caller.SendAsync("ReceiveConsumedMessagesInRangeBulkListProcessingProgress", percentComplete);
                    } 
                }

                await Clients.Caller.SendAsync("ReceiveConsumedMessagesInRangeBulkList", bulkList);
                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message, ex);
            }
        }

        public async Task<string> GetMessagesInRange(DateTime afterThisTimeStamp, int maxAmount, DateTime beforeThisTimeStamp)
        {
            try
            {
                var newMessages = GetMessageListInRange(afterThisTimeStamp, maxAmount, beforeThisTimeStamp);
                string lastDateRead = string.Empty; 
                 
                foreach (var message in newMessages)
                {
                    await Clients.Caller.SendAsync("ReceiveMessagesInRange", JsonMarshaller.Marshall(message));
                    lastDateRead = message.CreatedDateTime.ToString("yyyy-MM-dd HH:mm:ss.fffffff"); 
                }
                return lastDateRead;
            }
            catch (Exception ex)
            { 
                throw new ApplicationException(ex.Message, ex);
            }
        }
         
        public async Task<string> ConsumeMessagesInRange(DateTime afterThisTimeStamp, int maxAmount, DateTime beforeThisTimeStamp)
        {
            try
            {                
                var newMessages = GetMessageListInRange(afterThisTimeStamp, maxAmount, beforeThisTimeStamp);
                string lastDateRead = string.Empty;

                foreach (var message in newMessages)
                {    
                    //NOTE: Messages must have an Adapter type defined in order to be consumed.
                    if (String.IsNullOrEmpty(message.AdapterAssemblyQualifiedName) == false) 
                    {
                        string consumedMessage = _consumerAdapterFactory.Consume(message);
                        await Clients.Caller.SendAsync("ReceiveConsumedMessagesInRange", consumedMessage);
                    }

                    lastDateRead = message.CreatedDateTime.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
                }
                return lastDateRead;
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message, ex);
            }
        } 

        public async Task<bool> SubscribeToLatestMessage()
        {
            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, GroupNameLatestMessage); 
                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message, ex);
            }
        }

        public async Task<bool> UnSubscribeToLatestMessage()
        {
            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupNameLatestMessage);
                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message, ex);
            }
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message, ex);
            }
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            try
            {
                await base.OnDisconnectedAsync(ex);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupNameLatestMessage);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message, e);
            }
        }
    }
}

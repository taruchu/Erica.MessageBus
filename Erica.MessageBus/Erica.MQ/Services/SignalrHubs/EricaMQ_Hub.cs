using Erica.MQ.Interfaces.Factory;
using Erica.MQ.Services.SQL;
using EricaChats.DataAccess.Models;
using EricaMQ.Helpers;
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


        private List<IEricaMQ_MessageDTO> GetMessageListInRange(DateTime afterThisTimeStamp, int maxAmount, DateTime beforeThisTimeStamp)
        {
            var newMessages = _ericaMQ_DBContext.GET(afterThisTimeStamp, maxAmount, DateTime.MaxValue);
            return newMessages;
        }

         
        public async Task<string> GetMessagesInRange(DateTime afterThisTimeStamp, int maxAmount, DateTime beforeThisTimeStamp)
        {
            try
            {
                var newMessages = GetMessageListInRange(afterThisTimeStamp, maxAmount, beforeThisTimeStamp);
                string lastDateRead = string.Empty;

                //TODO: Create a batch call that will send a large list of messages at once, but only to the caller.
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

using Erica.MQ.Consumer.Helpers;
using Erica.MQ.Models.SQL;
using EricaChats.DataAccess.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using SharedInterfaces.Interfaces.DataTransferObjects;
using SharedInterfaces.Interfaces.EricaChats;
using System;
using System.Threading.Tasks;

namespace Erica.MQ.Consumer.Services.SignalrHubs
{
    public class EricaConsumer_Hub : Hub
    { 
        private IEricaChatsSimpleConsumerAdapter _ericaChatsSimpleConsumerAdapter { get; set; }
        public static string GroupNameLatestMessage = "latestMessageGroup"; 


        public EricaConsumer_Hub(IEricaChatsSimpleConsumerAdapter ericaChatsSimpleConsumerAdapter)
        {
            _ericaChatsSimpleConsumerAdapter = ericaChatsSimpleConsumerAdapter;              
        }
        
        public async Task<string> GetNewMessages(DateTime afterThisTimeStamp, int maxAmount)
        {
            try
            {
                string lastDateRead = string.Empty;

                HubConnection connection = new HubConnectionBuilder()
                    .WithUrl("http://localhost:80/api/ericamqhub/getnewmessages")
                    .Build();

                connection.On<string>("ReceiveNewMessage", (message) =>
                {
                    IEricaMQ_MessageDTO ericaMqDTO = JsonMarshaller.UnMarshall<EricaMQ_Message>(message);
                    IEricaChats_MessageDTO consumedMessage = (EricaChats_MessageDTO)_ericaChatsSimpleConsumerAdapter.Consume(ericaMqDTO);
                    Clients.Caller.SendAsync("ReceiveNewMessage", JsonMarshaller.Marshall(consumedMessage)).Wait();
                });

                await connection.StartAsync();

                Task<string> messageTask = connection.InvokeAsync<string>("GetNewMessages", afterThisTimeStamp, maxAmount);
                await messageTask;
                switch (messageTask.Status)
                {
                    case TaskStatus.Faulted:
                        throw new ApplicationException(messageTask.Exception.Flatten().InnerException.Message, messageTask.Exception.Flatten().InnerException);
                    case TaskStatus.RanToCompletion:
                        lastDateRead = messageTask.Result;
                        break;
                }

                await connection.StopAsync();
                return lastDateRead;
            }
            catch (Exception ex)
            { 
                throw new ApplicationException(ex.Message, ex);
            }
        }

        public async Task<string> GetNewMessagesInRange(DateTime afterThisTimeStamp, int maxAmount, DateTime beforeThisTimeStamp)
        {
            //TODO: Create a batch call that will send a large list of messages at once, but only to the caller.
            //Maybe use the streaming feature ? Does this give a progress measurement ?

            try
            {
                string lastDateRead = string.Empty;

                HubConnection connection = new HubConnectionBuilder()
                    .WithUrl("http://localhost:80/api/ericamqhub/getnewmessages")
                    .Build();

                connection.On<string>("ReceiveNewMessagesInRange", (message) =>
                {
                    IEricaMQ_MessageDTO ericaMqDTO = JsonMarshaller.UnMarshall<EricaMQ_Message>(message);
                    IEricaChats_MessageDTO consumedMessage = (EricaChats_MessageDTO)_ericaChatsSimpleConsumerAdapter.Consume(ericaMqDTO);
                    Clients.Caller.SendAsync("ReceiveNewMessagesInRange", JsonMarshaller.Marshall(consumedMessage)).Wait();
                });

                await connection.StartAsync();

                Task<string> messageTask = connection.InvokeAsync<string>("GetNewMessagesInRange", afterThisTimeStamp, maxAmount, beforeThisTimeStamp);
                await messageTask;
                switch (messageTask.Status)
                {
                    case TaskStatus.Faulted:
                        throw new ApplicationException(messageTask.Exception.Flatten().InnerException.Message, messageTask.Exception.Flatten().InnerException);
                    case TaskStatus.RanToCompletion:
                        lastDateRead = messageTask.Result;
                        break;
                }

                await connection.StopAsync();
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

        public async Task SendLatestMessage(EricaMQ_Message message)
        { 
            EricaChats_MessageDTO consumedMessage = (EricaChats_MessageDTO)_ericaChatsSimpleConsumerAdapter.Consume(message);
            await Clients.Groups(GroupNameLatestMessage).SendAsync("ReceiveLatestMessage", JsonMarshaller.Marshall(consumedMessage)); 
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
                throw new ApplicationException(ex.Message, e);
            }
        }
    }
}

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
    public class EricaChats_Hub : Hub
    { 
        private IEricaChatsSimpleConsumerAdapter _ericaChatsSimpleConsumerAdapter { get; set; }

        public EricaChats_Hub(IEricaChatsSimpleConsumerAdapter ericaChatsSimpleConsumerAdapter)
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


        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            await base.OnDisconnectedAsync(ex);
        }
    }
}

using Erica.MQ.Services.SQL;
using EricaMQ.Helpers;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Erica.MQ.Services.SignalrHubs
{
    public class EricaMQ_Hub : Hub
    {
        private EricaMQ_DBContext _ericaMQ_DBContext { get; set; }
        public bool _isConnected { get; set; }

        public EricaMQ_Hub(EricaMQ_DBContext ericaMQ_DBContext)
        {
            _ericaMQ_DBContext = ericaMQ_DBContext;
        }

        public async Task<string> GetNewMessages(DateTime afterThisTimeStamp, int maxAmount)
        {
            var newMessages = _ericaMQ_DBContext.GET(afterThisTimeStamp, maxAmount);
            string lastDateRead = string.Empty;
                           
            foreach (var message in newMessages)
            { 
                await Clients.Caller.SendAsync("ReceiveNewMessage", JsonMarshaller.Marshall(message));
                lastDateRead = message.CreatedDateTime.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
            }
            return lastDateRead;
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

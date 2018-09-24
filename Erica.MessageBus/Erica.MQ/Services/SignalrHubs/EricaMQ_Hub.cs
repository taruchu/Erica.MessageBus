using Erica.MQ.Services.SQL;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Erica.MQ.Services.SignalrHubs
{
    public class EricaMQ_Hub : Hub
    {
        private EricaMQ_DBContext _ericaMQ_DBContext { get; set; }
        private bool _isConnected { get; set; }

        public EricaMQ_Hub(EricaMQ_DBContext ericaMQ_DBContext)
        {
            _ericaMQ_DBContext = ericaMQ_DBContext;
        }

        public async Task GetNewMessages(DateTime afterThisTimeStamp, int maxAmount)
        {
            var newMessages = _ericaMQ_DBContext.GET(afterThisTimeStamp, maxAmount);

            DateTime lastTimeStampSent = afterThisTimeStamp; //NOTE: Seed the time stamp
            while (_isConnected)
            {
               var tryGetLastTimeStampSent = newMessages
                    .OrderByDescending(msg => msg.CreatedDateTime)
                    .Select(msg => msg.CreatedDateTime)
                    .FirstOrDefault();
                lastTimeStampSent = (tryGetLastTimeStampSent == DateTime.MinValue) ? lastTimeStampSent : tryGetLastTimeStampSent;
                foreach (var message in newMessages)
                {
                    await Clients.All.SendAsync("ReceiveNewMessage", message); 
                }
                newMessages.Clear();
                newMessages = _ericaMQ_DBContext.GET(lastTimeStampSent, maxAmount);
            }
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            _isConnected = true;
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
            _isConnected = false;
        }
    }
}

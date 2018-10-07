using Erica.MQ.Services.SignalrHubs;
using Erica.MQ.Services.SQL;
using EricaMQ.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using SharedInterfaces.Interfaces.DataTransferObjects;

namespace Erica.MQ.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class EricaMQController : ControllerBase
    {
        private EricaMQ_DBContext _ericaMQ_DBContext { get; set; }
        private IHubContext<EricaMQ_Hub> _hubContext { get; set; }

        public EricaMQController(EricaMQ_DBContext ericaMQ_DBContext, IHubContext<EricaMQ_Hub> hubContext)
        {
            _ericaMQ_DBContext = ericaMQ_DBContext;
            _hubContext = hubContext;
        }

        [HttpPost]
        public JsonResult Post(IEricaMQ_MessageDTO ericaMQ_Message)
        {
            var recipt = _ericaMQ_DBContext.POST(ericaMQ_Message);
            _hubContext.Clients.Group(EricaMQ_Hub.GroupNameLatestMessage).SendAsync("ReceiveLatestMessage", JsonMarshaller.Marshall(recipt));

            HubConnection consumedMessageConnection = new HubConnectionBuilder()
                     .WithUrl("http://localhost:5000/api/ericachatshub/getnewmessages")
                     .Build();
            consumedMessageConnection.StartAsync().Wait();
            consumedMessageConnection.InvokeAsync("SendLatestMessage", recipt).Wait();
            consumedMessageConnection.StopAsync().Wait();
            consumedMessageConnection.DisposeAsync().Wait();

            return new JsonResult(recipt, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
        }

        [HttpPut]
        public JsonResult Put(IEricaMQ_MessageDTO ericaMQ_Message)
        {
            var recipt = _ericaMQ_DBContext.PUT(ericaMQ_Message);
            _hubContext.Clients.Group(EricaMQ_Hub.GroupNameLatestMessage).SendAsync("ReceiveLatestMessage", JsonMarshaller.Marshall(recipt));

            HubConnection consumedMessageConnection = new HubConnectionBuilder()
                     .WithUrl("http://localhost:5000/api/ericachatshub/getnewmessages")
                     .Build();
            consumedMessageConnection.StartAsync().Wait();
            consumedMessageConnection.InvokeAsync("SendLatestMessage", recipt).Wait();
            consumedMessageConnection.StopAsync().Wait();
            consumedMessageConnection.DisposeAsync().Wait();

            return new JsonResult(recipt, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
        }
    }
}
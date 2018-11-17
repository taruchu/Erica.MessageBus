using Erica.MQ.Interfaces.Factory;
using Erica.MQ.Services.SignalrHubs;
using Erica.MQ.Services.SQL;
using EricaMQ.Helpers; 
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SharedInterfaces.Constants.EricaMQ_Hub;
using SharedInterfaces.Interfaces.DataTransferObjects;
using System;
using System.Reflection;

namespace Erica.MQ.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EricaMQController : ControllerBase
    {
        private EricaMQ_DBContext _ericaMQ_DBContext { get; set; }
        private IHubContext<EricaMQ_Hub> _hubContext { get; set; }
        private IConsumerAdapterFactory _consumerAdapterFactory { get; set; }
        private static ILogger _logger { get; set; }
        public EricaMQController(EricaMQ_DBContext ericaMQ_DBContext, IHubContext<EricaMQ_Hub> hubContext
            , IConsumerAdapterFactory consumerAdapterFactory, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(Assembly.GetExecutingAssembly().FullName);
            _ericaMQ_DBContext = ericaMQ_DBContext;
            _hubContext = hubContext;
            _consumerAdapterFactory = consumerAdapterFactory;
        }

        [HttpPost]
        public JsonResult Post(IEricaMQ_MessageDTO ericaMQ_Message)
        {
            try
            {
                var recipt = _ericaMQ_DBContext.POST(ericaMQ_Message);
                NotifyConsumers(recipt); 

                return new JsonResult(recipt, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw new ApplicationException(ex.Message, ex);
            }
        }

        [HttpPut]
        public JsonResult Put(IEricaMQ_MessageDTO ericaMQ_Message)
        {
            try
            {
                var recipt = _ericaMQ_DBContext.PUT(ericaMQ_Message);
                NotifyConsumers(recipt);  

                return new JsonResult(recipt, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw new ApplicationException(ex.Message, ex);
            }
        }

        private void NotifyConsumers(IEricaMQ_MessageDTO message)
        {
            try
            {
                //NOTE: Notify the consumers using the type of the adapter stored on the message if one exists, otherwise just marshall the message
                // and send without passing through the adapter.
                if(string.IsNullOrEmpty(message.AdapterAssemblyQualifiedName))
                {
                    _hubContext.Clients.Group(Constants_EricaMQ_Hub.GroupName_LatestMessage).SendAsync(Constants_EricaMQ_Hub.ClientEvent_ReceiveLatestMessage, JsonMarshaller.Marshall(message));
                }
                else
                {
                    //TODO: Divide the Hub client groups into consumer types, i.e. EricaChatsReceiveLatestConsumedMessage. Then use the message.Context to branch and determine
                    //the Hub Client Group
                    string consumedMessage =  _consumerAdapterFactory.Consume(message);
                    _hubContext.Clients.Group(Constants_EricaMQ_Hub.GroupName_LatestMessage).SendAsync(Constants_EricaMQ_Hub.ClientEvent_ReceiveLatestConsumedMessage, consumedMessage);
                } 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw new ApplicationException(ex.Message, ex);
            } 
        }
    }
}
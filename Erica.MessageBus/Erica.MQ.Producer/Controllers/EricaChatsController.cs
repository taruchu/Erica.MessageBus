using EricaChats.DataAccess.Services.SQL;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SharedInterfaces.Interfaces.DataTransferObjects;
using SharedInterfaces.Interfaces.EricaChats;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Erica.MQ.Producer.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class EricaChatsController : ControllerBase
    {
        private IEricaChatsSimpleProducerAdapter _ericaChatsSimpleProducerAdapter { get; set; }
        private IHttpClientFactory _httpClientFactory { get; set; }

        public EricaChatsController(IEricaChatsSimpleProducerAdapter ericaChatsSimpleProducerAdapter, IHttpClientFactory httpClientFactory)
        {
            _ericaChatsSimpleProducerAdapter = ericaChatsSimpleProducerAdapter;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost]
        public JsonResult Post(IEricaChats_MessageDTO request)
        {
            try
            { 
                IEricaMQ_MessageDTO mqMessage = _ericaChatsSimpleProducerAdapter.Produce(request);
                string jsonRecipt = string.Empty;

                var mqRequest = new HttpRequestMessage(HttpMethod.Post, "http://localhost:80/api/ericamq");
                mqRequest.Headers.Add("Content-Type", "application/json");
                mqRequest.Headers.Add("User-Agent", "EricaChatsController");
                mqRequest.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(jsonRecipt));

                var client = _httpClientFactory.CreateClient();
                HttpResponseMessage mqResponse = null;
                string errorLog = string.Empty; //TODO: Add Log4Net
                Task<HttpResponseMessage> mqTask = client.SendAsync(mqRequest); 
                mqTask.Wait();

                switch (mqTask.Status)
                {
                    case TaskStatus.Faulted:
                        DateTime timeStamp = DateTime.Now;
                        errorLog = $"Error while Posting to EricaMQ at {timeStamp} - Details on next line \n {mqTask.Exception.Flatten().InnerException.Message}";  
                        jsonRecipt = $"Could not post your request to the erica message queue.{timeStamp}";
                        break;
                    case TaskStatus.RanToCompletion:
                        mqResponse = mqTask.Result;
                        if (mqResponse.IsSuccessStatusCode)
                            jsonRecipt = mqMessage.Data;
                        break;
                    default:
                        jsonRecipt = $"Could not post your request to the erica message queue.{DateTime.Now}";
                        //TODO log the request payload with a timestamp
                        break;
                }
                  
                return new JsonResult(jsonRecipt, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message, ex);
            }
        }

        [HttpPut]
        public JsonResult Put(IEricaChats_MessageDTO request)
        { 
            //TODO
            return new JsonResult("", new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
        }
    }
}
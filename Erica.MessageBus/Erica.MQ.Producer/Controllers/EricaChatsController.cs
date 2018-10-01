using Erica.MQ.ProducerAdapter.Helpers;
using EricaChats.DataAccess.Models;
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

            if (request.ChatMessageID > 0)
                return new JsonResult("ChatMessageID must be 0 on a POST requst", new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });
            else
                return ProcessRequest(request);
        }

        [HttpPut]
        public JsonResult Put(IEricaChats_MessageDTO request)
        {
            if (request.ChatMessageID > -1)
                return new JsonResult("ChatMessageID must be greater than 0 on a PUT requst", new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });
            else
                return ProcessRequest(request);
        }

        private JsonResult ProcessRequest(IEricaChats_MessageDTO request)
        {
            try
            {
                IEricaMQ_MessageDTO mqMessage = _ericaChatsSimpleProducerAdapter.Produce(request);
                string jsonMqMessage = JsonMarshaller.Marshall(mqMessage);
                string jsonRecipt = string.Empty;

                var mqRequest = new HttpRequestMessage(HttpMethod.Post, "http://localhost:80/api/ericamq");
                mqRequest.Headers.Add("Content-Type", "application/json");
                mqRequest.Headers.Add("User-Agent", "EricaChatsController");
                mqRequest.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(jsonMqMessage));

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
                        IEricaChats_MessageDTO ericaChatsResponse = JsonMarshaller.UnMarshall<EricaChats_MessageDTO>(mqMessage.Data);
                        ericaChatsResponse.ErrorMessage = $"Could not post your request to the erica message queue.{timeStamp}";
                        jsonRecipt = JsonMarshaller.Marshall(ericaChatsResponse);
                        break;
                    case TaskStatus.RanToCompletion:
                        mqResponse = mqTask.Result;
                        if (mqResponse.IsSuccessStatusCode)
                            jsonRecipt = mqMessage.Data;
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
    }
}
using Erica.MQ.ProducerAdapter.Helpers;
using EricaChats.DataAccess.Models;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SharedInterfaces.Constants.IdentityServer;
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
    [Authorize]
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
        public async Task<JsonResult> Post(IEricaChats_MessageDTO request)
        {
            if (request.ChatMessageID != 0)
            {
                request.ErrorMessage = "ChatMessageID must be 0 on a POST requst";  
                return new JsonResult(request, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });
            } 
            else
                return await ProcessRequest(request);
        }

        [HttpPut]
        public async Task<JsonResult> Put(IEricaChats_MessageDTO request)
        {
            if (request.ChatMessageID < 1)
            {
                request.ErrorMessage = "ChatMessageID must be greater than 0 on a PUT requst";
                return new JsonResult(request, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto
                }); 
            }
            else 
               return await ProcessRequest(request);            
        }

        private async Task<JsonResult> ProcessRequest(IEricaChats_MessageDTO request)
        {
            try
            {
                IEricaMQ_MessageDTO mqMessage = _ericaChatsSimpleProducerAdapter.Produce(request); 
                string jsonMqMessage = JsonMarshaller.Marshall(mqMessage);
                IEricaChats_MessageDTO jsonRecipt = JsonMarshaller.UnMarshall<EricaChats_MessageDTO>(mqMessage.Data);

                var discover = await DiscoveryClient.GetAsync(Constants.IdentityServerUrl);
                if (discover.IsError)
                    throw new ApplicationException(discover.Error);

                var tokenClient = new TokenClient(discover.TokenEndpoint, Constants.EricaMQProducer_Client, Constants.EricaMQProducer_ClientSecret);
                var tokenResponse = await tokenClient.RequestClientCredentialsAsync(Constants.EricaMQ_Api);
                if (tokenResponse.IsError)
                    throw new ApplicationException(tokenResponse.Error);

                var client = _httpClientFactory.CreateClient();
                client.SetBearerToken(tokenResponse.AccessToken);
                var content = new StringContent(jsonMqMessage, Encoding.UTF8, "application/json");
                Task<HttpResponseMessage> mqTask = client.PostAsync("http://localhost:80/api/ericamq", content);

                HttpResponseMessage mqResponse = null;
                string errorLog = string.Empty; //TODO: Add Log4Net 
                await mqTask;

                switch (mqTask.Status)
                {
                    case TaskStatus.Faulted:
                        DateTime timeStamp = DateTime.Now;
                        errorLog = $"Error while Posting to EricaMQ at {timeStamp} - Details on next line \n {mqTask.Exception.Flatten().InnerException.Message}";
                        jsonRecipt.ErrorMessage = $"Could not post your request to the erica message queue. {timeStamp}";
                        break;
                    case TaskStatus.RanToCompletion:
                        mqResponse = mqTask.Result;
                        if (mqResponse.IsSuccessStatusCode == false)
                        {
                            DateTime timeStampStatusCode = DateTime.Now;
                            jsonRecipt.ErrorMessage = errorLog = $"Your request to the EricaMQ returned a status code of {mqResponse.StatusCode} at time {timeStampStatusCode}"; 
                        }
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
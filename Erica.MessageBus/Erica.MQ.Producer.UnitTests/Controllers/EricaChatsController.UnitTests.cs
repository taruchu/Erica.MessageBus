using SharedInterfaces.Helpers;
using EricaChats.DataAccess.Models;
using IdentityModel.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharedInterfaces.Interfaces.EricaChats;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SharedInterfaces.Constants.IdentityServer;

namespace Erica.MQ.Producer.UnitTests
{
    [TestClass]
    public class EricaChatsControllerUnitTests
    {
        private async Task<HttpResponseMessage> SendRequest(string jsonEricaChatsMessage, bool put = false)
        {
            try
            {
                //Authenticate
                var disco = await DiscoveryClient.GetAsync(Constants_IdentityServer.IdentityServerUrl);
                if (disco.IsError)
                    throw new ApplicationException(disco.Error);

                var tokenClient = new TokenClient(disco.TokenEndpoint, Constants_IdentityServer.ExternalClient, Constants_IdentityServer.ExternalClient_ClientSecret);
                var tokenResponse = await tokenClient.RequestClientCredentialsAsync(Constants_IdentityServer.EricaMQProducer_Api);
                if (tokenResponse.IsError)
                    throw new ApplicationException(tokenResponse.Error);

                var client = new HttpClient();
                client.SetBearerToken(tokenResponse.AccessToken); 
                var content = new StringContent(jsonEricaChatsMessage, Encoding.UTF8, "application/json");
                Task<HttpResponseMessage> mqTask = (put == false) ? client.PostAsync("http://localhost:50000/api/ericachats", content) :
                    client.PutAsync("http://localhost:50000/api/ericachats", content);

                HttpResponseMessage mqProducerResponse = await mqTask;

                return mqProducerResponse;
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message, ex);
            }
        }

        private string GetContentBody(HttpResponseMessage httpResponseMessage)
        {
            string contentBody = string.Empty; 
            Task<string> contentTask = httpResponseMessage.Content.ReadAsStringAsync();
            contentTask.Wait();
            switch (contentTask.Status)
            {
                case TaskStatus.Faulted:
                    throw new ApplicationException(contentTask.Exception.Flatten().InnerException.Message, contentTask.Exception.Flatten().InnerException);
                case TaskStatus.RanToCompletion:
                    contentBody = contentTask.Result;
                    break;
            } 
            return contentBody;
        }

        private HttpResponseMessage GetRequestResponse(string request, bool put = false)
        {
            HttpResponseMessage response = null;
            var requestTask = SendRequest(request, put);
            requestTask.Wait();
            switch (requestTask.Status)
            {
                case TaskStatus.Faulted:
                    throw new ApplicationException(requestTask.Exception.Flatten().InnerException.Message, requestTask.Exception.Flatten().InnerException);
                case TaskStatus.RanToCompletion:
                    response = requestTask.Result;
                    break;
            }
            return response;
        }

        [TestMethod]
        public void TestPost()
        {
            IEricaChats_MessageDTO ericaDTO = new EricaChats_MessageDTO();
            ericaDTO.ChatChannelID = 1;
            ericaDTO.ChatChannelName = "Christ";
            ericaDTO.SenderUserName = "Jesus";
            ericaDTO.ChatMessageBody = "Love thy neighbore as thyself";
            string mqRequest = JsonMarshaller.Marshall(ericaDTO);
            HttpResponseMessage response = GetRequestResponse(mqRequest);
            Assert.IsTrue(response.IsSuccessStatusCode); 
            string contentBody = GetContentBody(response);           

            Assert.IsFalse(String.IsNullOrEmpty(contentBody));
            IEricaChats_MessageDTO mqProducerResponse = JsonMarshaller.UnMarshall<EricaChats_MessageDTO>(contentBody);
            Assert.IsNotNull(mqProducerResponse);
            Assert.IsTrue(String.IsNullOrEmpty(mqProducerResponse.ErrorMessage));
        }

        [TestMethod]
        public void TestPut()
        {
            IEricaChats_MessageDTO ericaDTO = new EricaChats_MessageDTO();
            ericaDTO.ChatChannelID = 1;
            ericaDTO.ChatChannelName = "Christ";
            ericaDTO.SenderUserName = "Jesus";
            ericaDTO.ChatMessageBody = "Love thy neighbore as thyself";
            string mqRequest = JsonMarshaller.Marshall(ericaDTO);
            HttpResponseMessage response = GetRequestResponse(mqRequest); 
            Assert.IsTrue(response.IsSuccessStatusCode); 
            string contentBody = GetContentBody(response);            

            Assert.IsFalse(String.IsNullOrEmpty(contentBody));
            IEricaChats_MessageDTO mqProducerResponse = JsonMarshaller.UnMarshall<EricaChats_MessageDTO>(contentBody);
            Assert.IsNotNull(mqProducerResponse);

            string newSenderUserName = "God is Jesus Christ";
            mqProducerResponse.SenderUserName = newSenderUserName;

            //******* Test the PUT
            string mqUpdatedRequest = JsonMarshaller.Marshall(mqProducerResponse);
            HttpResponseMessage updatedResponse = GetRequestResponse(mqUpdatedRequest, true); 
            Assert.IsTrue(updatedResponse.IsSuccessStatusCode); 
            string updatedContentBody = GetContentBody(updatedResponse);            

            Assert.IsFalse(String.IsNullOrEmpty(updatedContentBody));
            IEricaChats_MessageDTO updatedMqProducerResponse = JsonMarshaller.UnMarshall<EricaChats_MessageDTO>(updatedContentBody);
            Assert.AreEqual(newSenderUserName, updatedMqProducerResponse.SenderUserName);
            Assert.IsTrue(String.IsNullOrEmpty(updatedMqProducerResponse.ErrorMessage));
        }
    }
}

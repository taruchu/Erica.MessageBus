using Erica.MQ.Models.SQL;
using Erica.MQ.UnitTests.Helpers;
using EricaChats.DataAccess.Models;
using IdentityModel.Client;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharedInterfaces.Interfaces.DataTransferObjects;
using SharedInterfaces.Interfaces.EricaChats;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SharedInterfaces.Constants.IdentityServer;
using SharedInterfaces.Constants.EricaMQ_Hub;

namespace EricaMQ.UnitTests.Controllers
{
    [TestClass]
    public class EricaMQControllerUnitTests
    {
        private async Task<HttpResponseMessage> SendRequest(string jsonMqMessage, bool put = false)
        {
            try
            {
                var tokenResponse = await GetAccessToken();

                var client = new HttpClient();
                client.SetBearerToken(tokenResponse.AccessToken);
                var content = new StringContent(jsonMqMessage, Encoding.UTF8, "application/json");
                Task<HttpResponseMessage> mqTask = (put == false) ? client.PostAsync("http://localhost:80/api/ericamq", content) :
                   client.PutAsync("http://localhost:80/api/ericamq", content); 
                HttpResponseMessage mqResponse = await mqTask;
                
                return mqResponse;
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message, ex);
            }
        }

        private async Task<TokenResponse> GetAccessToken()
        {
            //Authenticate
            var disco = await DiscoveryClient.GetAsync(Constants_IdentityServer.IdentityServerUrl);
            if (disco.IsError)
                throw new ApplicationException(disco.Error);

            var tokenClient = new TokenClient(disco.TokenEndpoint, Constants_IdentityServer.EricaMQProducer_Client, Constants_IdentityServer.EricaMQProducer_ClientSecret);
            var tokenResponse = await tokenClient.RequestClientCredentialsAsync(Constants_IdentityServer.EricaMQ_Api);
            if (tokenResponse.IsError)
                throw new ApplicationException(tokenResponse.Error);
            return tokenResponse;
        }

        [TestMethod]
        public void TestPost()
        {
            IEricaMQ_MessageDTO ericaMQ_MessageDTO = new EricaMQ_Message();
            ericaMQ_MessageDTO.Context = "UnitTest";
            ericaMQ_MessageDTO.Data = "UnitTest";
            ericaMQ_MessageDTO.Sender = "UnitTest";
            string mqRequest = JsonMarshaller.Marshall(ericaMQ_MessageDTO);
            HttpResponseMessage response = null;
            var requestTask = SendRequest(mqRequest);
            requestTask.Wait();
            switch (requestTask.Status)
            { 
                case TaskStatus.Faulted:
                    throw new ApplicationException(requestTask.Exception.Flatten().InnerException.Message, requestTask.Exception.Flatten().InnerException);
                case TaskStatus.RanToCompletion:
                    response = requestTask.Result;
                    break; 
            } 

            Assert.IsTrue(response.IsSuccessStatusCode);

            string contentBody = string.Empty;
            Task<string> contentTask = response.Content.ReadAsStringAsync();
            contentTask.Wait();
            switch (contentTask.Status)
            { 
                case TaskStatus.Faulted:
                    throw new ApplicationException(contentTask.Exception.Flatten().InnerException.Message, contentTask.Exception.Flatten().InnerException); 
                case TaskStatus.RanToCompletion:
                    contentBody = contentTask.Result;
                    break; 
            }

            Assert.IsFalse(String.IsNullOrEmpty(contentBody));
            IEricaMQ_MessageDTO mqResponse = JsonMarshaller.UnMarshall<EricaMQ_Message>(contentBody);
            Assert.IsNotNull(mqResponse);
        }

        [TestMethod]
        public void TestPut()
        {
            try
            {
                //POST
                IEricaMQ_MessageDTO ericaMQ_MessageDTO = new EricaMQ_Message();
                ericaMQ_MessageDTO.Context = "UnitTest";
                ericaMQ_MessageDTO.Data = "UnitTest";
                ericaMQ_MessageDTO.Sender = "UnitTest";
                string mqRequest = JsonMarshaller.Marshall(ericaMQ_MessageDTO);
                HttpResponseMessage response = null;
                var requestTask = SendRequest(mqRequest);
                requestTask.Wait();
                switch (requestTask.Status)
                {
                    case TaskStatus.Faulted:
                        throw new ApplicationException(requestTask.Exception.Flatten().InnerException.Message, requestTask.Exception.Flatten().InnerException);
                    case TaskStatus.RanToCompletion:
                        response = requestTask.Result;
                        break;
                }

                Assert.IsTrue(response.IsSuccessStatusCode);

                string contentBody = string.Empty;
                Task<string> contentTask = response.Content.ReadAsStringAsync();
                contentTask.Wait();
                switch (contentTask.Status)
                {
                    case TaskStatus.Faulted:
                        throw new ApplicationException(contentTask.Exception.Flatten().InnerException.Message, contentTask.Exception.Flatten().InnerException);
                    case TaskStatus.RanToCompletion:
                        contentBody = contentTask.Result;
                        break;
                }

                Assert.IsFalse(String.IsNullOrEmpty(contentBody));
                IEricaMQ_MessageDTO mqResponse = JsonMarshaller.UnMarshall<EricaMQ_Message>(contentBody);
                Assert.IsNotNull(mqResponse);

                //PUT
                string newData = "Love";
                mqResponse.Data = newData;

                string updatedJsonMqRequest = JsonMarshaller.Marshall(mqResponse);
                HttpResponseMessage updatedMqResponseMessage = null;
                var updatedMqRequestMessageTask = SendRequest(updatedJsonMqRequest, true);
                updatedMqRequestMessageTask.Wait();
                switch (updatedMqRequestMessageTask.Status)
                {
                    case TaskStatus.Faulted:
                        throw new ApplicationException(updatedMqRequestMessageTask.Exception.Flatten().InnerException.Message, updatedMqRequestMessageTask.Exception.Flatten().InnerException);
                    case TaskStatus.RanToCompletion:
                        updatedMqResponseMessage = updatedMqRequestMessageTask.Result;
                        break;
                }

                Assert.IsTrue(updatedMqResponseMessage.IsSuccessStatusCode);

                string updatedContentBody = string.Empty;
                Task<string> updatedContentTask = updatedMqResponseMessage.Content.ReadAsStringAsync();
                updatedContentTask.Wait();
                switch (updatedContentTask.Status)
                {
                    case TaskStatus.Faulted:
                        throw new ApplicationException(updatedContentTask.Exception.Flatten().InnerException.Message, updatedContentTask.Exception.Flatten().InnerException);
                    case TaskStatus.RanToCompletion:
                        updatedContentBody = updatedContentTask.Result;
                        break;
                }

                Assert.IsFalse(String.IsNullOrEmpty(updatedContentBody));
                IEricaMQ_MessageDTO updatedMqMessage = JsonMarshaller.UnMarshall<EricaMQ_Message>(updatedContentBody);
                Assert.IsNotNull(updatedMqMessage);
                Assert.AreEqual(newData, updatedMqMessage.Data);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [TestMethod]
        public void TestGetNewMessagesHub()
        {
            TokenResponse tokenResponse = null;
            var tokenResponseTask = GetAccessToken();
            tokenResponseTask.Wait();
            switch (tokenResponseTask.Status)
            { 
                case TaskStatus.Faulted:
                    throw new ApplicationException(tokenResponseTask.Exception.Flatten().InnerException.Message, tokenResponseTask.Exception.Flatten().InnerException);
                case TaskStatus.RanToCompletion:
                    tokenResponse = tokenResponseTask.Result;                    
                    break; 
            }

            List<IEricaMQ_MessageDTO> newMessagesList = new List<IEricaMQ_MessageDTO>();
            List<IEricaChats_MessageDTO> newMessagesListConsume = new List<IEricaChats_MessageDTO>();

            HubConnection connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:80/api/ericamqhub", options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(tokenResponse.AccessToken);
                })
                .Build();

            connection.On<string>(Constants_EricaMQ_Hub.ClientEvent_ReceiveMessagesInRange, (message) =>
            {
                IEricaMQ_MessageDTO mqMessage = JsonMarshaller.UnMarshall<EricaMQ_Message>(message);
                Assert.IsNotNull(mqMessage);
                newMessagesList.Add(mqMessage);
            });

            connection.On<string>(Constants_EricaMQ_Hub.ClientEvent_ReceiveConsumedMessagesInRange, (message) =>
            {
                //NOTE: Client must know the expected type to be consumed
                IEricaChats_MessageDTO mqMessage = JsonMarshaller.UnMarshall<EricaChats_MessageDTO>(message);
                Assert.IsNotNull(mqMessage);
                newMessagesListConsume.Add(mqMessage);
            });

            connection.StartAsync().Wait(); 


            IEricaChats_MessageDTO ericaChats = new EricaChats_MessageDTO();
            ericaChats.ChatMessageID = 123;
            ericaChats.ChatChannelID = 1;
            ericaChats.ChatMessageBody = "Jesus Loves You. ";
            ericaChats.CreatedDateTime = DateTime.Now;
            ericaChats.ModifiedDateTime = DateTime.MinValue;
            ericaChats.SenderUserName = "God";            

            IEricaMQ_MessageDTO ericaMQ_MessageDTO = new EricaMQ_Message();
            ericaMQ_MessageDTO.Context = "UnitTest-Consumed";
            ericaMQ_MessageDTO.Data = JsonMarshaller.Marshall(ericaChats);
            ericaMQ_MessageDTO.Sender = "UnitTest-Producer";
            ericaMQ_MessageDTO.AdapterAssemblyQualifiedName = typeof(IEricaChatsSimpleConsumerAdapter).AssemblyQualifiedName;
            string mqRequest = JsonMarshaller.Marshall(ericaMQ_MessageDTO);
            HttpResponseMessage response = null;
            var requestTask = SendRequest(mqRequest);
            requestTask.Wait();
            switch (requestTask.Status)
            {
                case TaskStatus.Faulted:
                    throw new ApplicationException(requestTask.Exception.Flatten().InnerException.Message, requestTask.Exception.Flatten().InnerException);
                case TaskStatus.RanToCompletion:
                    response = requestTask.Result;
                    break;
            }

            DateTime afterTime = Convert.ToDateTime("2018-10-21 18:19:03.7618097");  //REMEMBER: Must choose this date wisely before running test (un-migrated data may cause test to fail)

            Task<string> messageTask = connection.InvokeAsync<string>(Constants_EricaMQ_Hub.HubMethod_GetMessagesInRange, afterTime, 200, DateTime.MaxValue);
            messageTask.Wait();
            switch (messageTask.Status)
            { 
                case TaskStatus.Faulted:
                    throw new ApplicationException(messageTask.Exception.Flatten().InnerException.Message, messageTask.Exception.Flatten().InnerException); 
                case TaskStatus.RanToCompletion:
                    afterTime = Convert.ToDateTime(messageTask.Result);
                    break; 
            } 

            afterTime = Convert.ToDateTime("2018-10-21 18:19:03.7618097"); //REMEMBER: Must choose this date wisely before running test (un-migrated data may cause test to fail)

            Task<string> messageTaskConsume = connection.InvokeAsync<string>(Constants_EricaMQ_Hub.HubMethod_ConsumeMessagesInRange, afterTime, 200, DateTime.MaxValue);
            messageTaskConsume.Wait();
            switch (messageTaskConsume.Status)
            {
                case TaskStatus.Faulted:
                    throw new ApplicationException(messageTaskConsume.Exception.Flatten().InnerException.Message, messageTaskConsume.Exception.Flatten().InnerException);
                case TaskStatus.RanToCompletion:
                    afterTime = Convert.ToDateTime(messageTaskConsume.Result);
                    break;
            } 

            connection.StopAsync().Wait();

            foreach (var message in newMessagesList)
            {
                Assert.IsTrue(message.Id > 0);
            }
             
            foreach (var message in newMessagesListConsume)
            {
                Assert.IsTrue(message.ChatMessageID > 0);
            } 
        }

        [TestMethod]
        public void TestGetLatestMessage()
        {
            TokenResponse tokenResponse = null;
            var tokenResponseTask = GetAccessToken();
            tokenResponseTask.Wait();
            switch (tokenResponseTask.Status)
            {
                case TaskStatus.Faulted:
                    throw new ApplicationException(tokenResponseTask.Exception.Flatten().InnerException.Message, tokenResponseTask.Exception.Flatten().InnerException);
                case TaskStatus.RanToCompletion:
                    tokenResponse = tokenResponseTask.Result;
                    break;
            }

            List<IEricaMQ_MessageDTO> newMessagesList = new List<IEricaMQ_MessageDTO>();
            List<IEricaChats_MessageDTO> newMessagesList2 = new List<IEricaChats_MessageDTO>();

            HubConnection connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:80/api/ericamqhub",
                    options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(tokenResponse.AccessToken);
                    }
                )
                .Build();

            connection.On<string>(Constants_EricaMQ_Hub.ClientEvent_ReceiveLatestMessage, (message) =>
            {
                IEricaMQ_MessageDTO mQ_MessageDTO = JsonMarshaller.UnMarshall<EricaMQ_Message>(message);
                Assert.IsNotNull(mQ_MessageDTO);
                newMessagesList.Add(mQ_MessageDTO);
            });

            connection.StartAsync().Wait();
            connection.InvokeAsync<bool>(Constants_EricaMQ_Hub.HubMethod_SubscribeToLatestMessage).Wait();

            TokenResponse tokenResponse2 = null;
            var tokenResponseTask2 = GetAccessToken();
            tokenResponseTask2.Wait();
            switch (tokenResponseTask2.Status)
            {
                case TaskStatus.Faulted:
                    throw new ApplicationException(tokenResponseTask2.Exception.Flatten().InnerException.Message, tokenResponseTask2.Exception.Flatten().InnerException);
                case TaskStatus.RanToCompletion:
                    tokenResponse2 = tokenResponseTask2.Result;
                    break;
            }

            HubConnection connection2 = new HubConnectionBuilder()
               .WithUrl("http://localhost:80/api/ericamqhub", 
                options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(tokenResponse.AccessToken);
                    }
               )
               .Build();

            connection2.On<string>(Constants_EricaMQ_Hub.ClientEvent_ReceiveLatestConsumedMessage, (message) =>
            {
                IEricaChats_MessageDTO mQ_MessageDTO2 = JsonMarshaller.UnMarshall<EricaChats_MessageDTO>(message);
                Assert.IsNotNull(mQ_MessageDTO2);
                newMessagesList2.Add(mQ_MessageDTO2);
            });

            connection2.StartAsync().Wait();
            connection2.InvokeAsync<bool>(Constants_EricaMQ_Hub.HubMethod_SubscribeToLatestMessage).Wait();

            while (newMessagesList.Count < 3 && newMessagesList2.Count < 3)
            {
                //POST
                IEricaChats_MessageDTO ericaChats = new EricaChats_MessageDTO();
                ericaChats.ChatMessageID = 123;
                ericaChats.ChatChannelID = 1;
                ericaChats.ChatMessageBody = "Jesus Loves You. ";
                ericaChats.CreatedDateTime = DateTime.Now;
                ericaChats.ModifiedDateTime = DateTime.MinValue;
                ericaChats.SenderUserName = "God";

                IEricaMQ_MessageDTO ericaMQ_MessageDTOConsume = new EricaMQ_Message();
                ericaMQ_MessageDTOConsume.Context = "UnitTestLatest";
                ericaMQ_MessageDTOConsume.Data = JsonMarshaller.Marshall(ericaChats);
                ericaMQ_MessageDTOConsume.Sender = "UnitTestLatest";
                ericaMQ_MessageDTOConsume.AdapterAssemblyQualifiedName = typeof(IEricaChatsSimpleConsumerAdapter).AssemblyQualifiedName;
                string mqRequestConsume = JsonMarshaller.Marshall(ericaMQ_MessageDTOConsume); 
                HttpResponseMessage responseConsume = null;
                var requestConsumeTask = SendRequest(mqRequestConsume);
                requestConsumeTask.Wait();
                switch (requestConsumeTask.Status)
                {
                    case TaskStatus.Faulted:
                        throw new ApplicationException(requestConsumeTask.Exception.Flatten().InnerException.Message, requestConsumeTask.Exception.Flatten().InnerException);
                    case TaskStatus.RanToCompletion:
                        responseConsume = requestConsumeTask.Result;
                        break;
                }

                Assert.IsTrue(responseConsume.IsSuccessStatusCode);

                IEricaMQ_MessageDTO ericaMQ_MessageDTO = new EricaMQ_Message();
                ericaMQ_MessageDTO.Context = "UnitTestLatest";
                ericaMQ_MessageDTO.Data = "UnitTestLatest";
                ericaMQ_MessageDTO.Sender = "UnitTestLatest"; 
                string mqRequest = JsonMarshaller.Marshall(ericaMQ_MessageDTO);
                HttpResponseMessage response = null;
                var requestTask = SendRequest(mqRequest);
                requestTask.Wait();
                switch (requestTask.Status)
                {
                    case TaskStatus.Faulted:
                        throw new ApplicationException(requestTask.Exception.Flatten().InnerException.Message, requestTask.Exception.Flatten().InnerException);
                    case TaskStatus.RanToCompletion:
                        response = requestTask.Result;
                        break;
                }
                Assert.IsTrue(response.IsSuccessStatusCode);
            }
             
            connection.StopAsync();
            connection2.StopAsync();

            foreach (var message in newMessagesList)
            {
                Assert.IsTrue(message.Id > 0); 
            }

            foreach (var message in newMessagesList2)
            {
                Assert.IsTrue(message.ChatMessageID > 0);
            }
        }


    } 
}

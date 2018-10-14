using Erica.MQ.Models.SQL;
using Erica.MQ.UnitTests.Helpers;
using EricaChats.DataAccess.Models;
using IdentityModel.Client;
using IdentityServer.IdentityServerConstants;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharedInterfaces.Interfaces.DataTransferObjects;
using SharedInterfaces.Interfaces.EricaChats;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EricaMQ.UnitTests.Controllers
{
    [TestClass]
    public class EricaMQControllerUnitTests
    {
        private async Task<HttpResponseMessage> SendRequest(string jsonMqMessage, bool put = false)
        {
            try
            {
                //Authenticate
                var disco = await DiscoveryClient.GetAsync(Constants.IdentityServerUrl);
                if (disco.IsError)
                    throw new ApplicationException(disco.Error);

                var tokenClient = new TokenClient(disco.TokenEndpoint, Constants.EricaMQProducer_Client, Constants.EricaMQProducer_ClientSecret);
                var tokenResponse = await tokenClient.RequestClientCredentialsAsync(Constants.EricaMQ_Api);
                if(tokenResponse.IsError) 
                    throw new ApplicationException(tokenResponse.Error); 


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
            List<IEricaMQ_MessageDTO> newMessagesList = new List<IEricaMQ_MessageDTO>();
            List<IEricaChats_MessageDTO> newMessagesListConsume = new List<IEricaChats_MessageDTO>();

            HubConnection connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:80/api/ericamqhub")
                .Build();

            connection.On<string>("ReceiveMessagesInRange", (message) =>
            {
                IEricaMQ_MessageDTO mqMessage = JsonMarshaller.UnMarshall<EricaMQ_Message>(message);
                Assert.IsNotNull(mqMessage);
                newMessagesList.Add(mqMessage);
            });

            connection.On<string>("ReceiveConsumedMessagesInRange", (message) =>
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
            ericaMQ_MessageDTO.AdapterAssemblyQualifiedName = typeof(IEricaChatsSimpleConsumerAdapter).ToString();
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

            DateTime afterTime = Convert.ToDateTime("2018-10-07 02:00:27.7893256"); 

             
            Task<string> messageTask = connection.InvokeAsync<string>("GetMessagesInRange", afterTime, 200, DateTime.MaxValue);
            messageTask.Wait();
            switch (messageTask.Status)
            { 
                case TaskStatus.Faulted:
                    throw new ApplicationException(messageTask.Exception.Flatten().InnerException.Message, messageTask.Exception.Flatten().InnerException); 
                case TaskStatus.RanToCompletion:
                    afterTime = Convert.ToDateTime(messageTask.Result);
                    break; 
            } 

            afterTime = Convert.ToDateTime("2018-10-07 02:00:27.7893256");

            Task<string> messageTaskConsume = connection.InvokeAsync<string>("ConsumeMessagesInRange", afterTime, 200, DateTime.MaxValue);
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
            List<IEricaMQ_MessageDTO> newMessagesList = new List<IEricaMQ_MessageDTO>();
            List<IEricaChats_MessageDTO> newMessagesList2 = new List<IEricaChats_MessageDTO>();


            HubConnection connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:80/api/ericamqhub")
                .Build();

            connection.On<string>("ReceiveLatestMessage", (message) =>
            {
                IEricaMQ_MessageDTO mQ_MessageDTO = JsonMarshaller.UnMarshall<EricaMQ_Message>(message);
                Assert.IsNotNull(mQ_MessageDTO);
                newMessagesList.Add(mQ_MessageDTO);
            });

            connection.StartAsync().Wait();
            connection.InvokeAsync<bool>("SubscribeToLatestMessage").Wait();

            HubConnection connection2 = new HubConnectionBuilder()
               .WithUrl("http://localhost:80/api/ericamqhub")
               .Build();

            connection2.On<string>("ReceiveLatestConsumedMessage", (message) =>
            {
                IEricaChats_MessageDTO mQ_MessageDTO2 = JsonMarshaller.UnMarshall<EricaChats_MessageDTO>(message);
                Assert.IsNotNull(mQ_MessageDTO2);
                newMessagesList2.Add(mQ_MessageDTO2);
            });

            connection2.StartAsync().Wait();
            connection2.InvokeAsync<bool>("SubscribeToLatestMessage").Wait();

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
                ericaMQ_MessageDTOConsume.AdapterAssemblyQualifiedName = typeof(IEricaChatsSimpleConsumerAdapter).ToString();
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

using Erica.MQ.Models.SQL;
using Erica.MQ.UnitTests.Helpers;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharedInterfaces.Interfaces.DataTransferObjects;
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
        private HttpResponseMessage SendRequest(string jsonMqMessage, bool put = false)
        {
            var client = new HttpClient();
            HttpResponseMessage mqResponse = null;
            var content = new StringContent(jsonMqMessage, Encoding.UTF8, "application/json");
            Task<HttpResponseMessage> mqTask = (put == false) ? client.PostAsync("http://localhost:80/api/ericamq", content) :
               client.PutAsync("http://localhost:80/api/ericamq", content);

            mqTask.Wait();
            switch (mqTask.Status)
            { 
                case TaskStatus.Faulted:
                    throw new ApplicationException(mqTask.Exception.Flatten().InnerException.Message, mqTask.Exception.Flatten().InnerException);
                case TaskStatus.RanToCompletion:
                    mqResponse = mqTask.Result;
                    break;
            }
            return mqResponse;
        }

        [TestMethod]
        public void TestPost()
        {
            IEricaMQ_MessageDTO ericaMQ_MessageDTO = new EricaMQ_Message();
            ericaMQ_MessageDTO.Context = "UnitTest";
            ericaMQ_MessageDTO.Data = "UnitTest";
            ericaMQ_MessageDTO.Sender = "UnitTest";
            string mqRequest = JsonMarshaller.Marshall(ericaMQ_MessageDTO);
            HttpResponseMessage response = SendRequest(mqRequest);

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
            //POST
            IEricaMQ_MessageDTO ericaMQ_MessageDTO = new EricaMQ_Message();
            ericaMQ_MessageDTO.Context = "UnitTest";
            ericaMQ_MessageDTO.Data = "UnitTest";
            ericaMQ_MessageDTO.Sender = "UnitTest";
            string mqRequest = JsonMarshaller.Marshall(ericaMQ_MessageDTO);
            HttpResponseMessage response = SendRequest(mqRequest);

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
            HttpResponseMessage updatedMqResponseMessage = SendRequest(updatedJsonMqRequest, true);

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

        [TestMethod]
        public void TestGetNewMessagesHub()
        {
            IEricaMQ_MessageDTO mqMessage;
            List<IEricaMQ_MessageDTO> newMessagesList = new List<IEricaMQ_MessageDTO>();

            HubConnection connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:80/api/ericamqhub/getnewmessages")
                .Build();

            connection.On<string>("ReceiveNewMessage", (message) =>
            {
                mqMessage = JsonMarshaller.UnMarshall<EricaMQ_Message>(message);
                Assert.IsNotNull(mqMessage);
                newMessagesList.Add(mqMessage);
            });
          
            DateTime afterTime = Convert.ToDateTime("2018-09-23 18:17:00.0543308");
            connection.StartAsync().Wait(); 

            while(newMessagesList.Count < 9)
            {
                Task<string> messageTask = connection.InvokeAsync<string>("GetNewMessages", afterTime, 3);
                messageTask.Wait();
                switch (messageTask.Status)
                { 
                    case TaskStatus.Faulted:
                        throw new ApplicationException(messageTask.Exception.Flatten().InnerException.Message, messageTask.Exception.Flatten().InnerException); 
                    case TaskStatus.RanToCompletion:
                        afterTime = Convert.ToDateTime(messageTask.Result);
                        break; 
                }
            }
                
             
            
            connection.StopAsync().Wait();

            foreach (var message in newMessagesList)
            {
                Assert.IsTrue(message.Id > 0);
            }

        }
    }
}

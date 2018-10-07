using Erica.MQ.Consumer.Helpers;
using Erica.MQ.Models.SQL;
using EricaChats.DataAccess.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharedInterfaces.Interfaces.DataTransferObjects;
using SharedInterfaces.Interfaces.EricaChats;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Erica.MQ.Consumer.Hub.UnitTests
{
    [TestClass]
    public class EricaChatsHubUnitTests
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
        public void GetLatestMessage()
        {
            IEricaChats_MessageDTO chats_MessageDTO;
            IEricaChats_MessageDTO chats_MessageDTO2;
            List<IEricaChats_MessageDTO> newMessagesList = new List<IEricaChats_MessageDTO>();
            List<IEricaChats_MessageDTO> newMessagesList2 = new List<IEricaChats_MessageDTO>();


            HubConnection connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/api/ericachatshub/getnewmessages")
                .Build();

            connection.On<string>("ReceiveLatestMessage", (message) =>
            {
                chats_MessageDTO = JsonMarshaller.UnMarshall<EricaChats_MessageDTO>(message);
                Assert.IsNotNull(chats_MessageDTO); 
                newMessagesList.Add(chats_MessageDTO);
            });

            connection.StartAsync().Wait();
            connection.InvokeAsync<bool>("SubscribeToLatestMessage").Wait();

            HubConnection connection2 = new HubConnectionBuilder()
               .WithUrl("http://localhost:5000/api/ericachatshub/getnewmessages")
               .Build();

            connection2.On<string>("ReceiveLatestMessage", (message) =>
            {
                chats_MessageDTO2 = JsonMarshaller.UnMarshall<EricaChats_MessageDTO>(message);
                Assert.IsNotNull(chats_MessageDTO2); 
                newMessagesList2.Add(chats_MessageDTO2);
            });

            connection2.StartAsync().Wait();
            connection2.InvokeAsync<bool>("SubscribeToLatestMessage").Wait();

            IEricaChats_MessageDTO ericaChats_Message = new EricaChats_MessageDTO();
            ericaChats_Message.ChatChannelID = 1;
            ericaChats_Message.ChatChannelName = "Jesus";
            ericaChats_Message.ChatMessageBody = "God";
            ericaChats_Message.ChatMessageID = 123;
            ericaChats_Message.CreatedDateTime = DateTime.Now;
            ericaChats_Message.ModifiedDateTime = DateTime.MinValue;
            string data = JsonMarshaller.Marshall(ericaChats_Message);
            int counter = 0;
            while (newMessagesList.Count < 3 && newMessagesList2.Count < 3)
            {
                //POST
                IEricaMQ_MessageDTO ericaMQ_MessageDTO = new EricaMQ_Message();
                ericaMQ_MessageDTO.Context = $"UnitTestLatest{counter++}";
                ericaMQ_MessageDTO.Data = data;
                ericaMQ_MessageDTO.Sender = "UnitTestLatest";
                string mqRequest = JsonMarshaller.Marshall(ericaMQ_MessageDTO);
                HttpResponseMessage response = SendRequest(mqRequest);

                Assert.IsTrue(response.IsSuccessStatusCode);
            }

            connection.StopAsync().Wait();
            connection.DisposeAsync().Wait();
            connection2.StopAsync().Wait();
            connection2.DisposeAsync().Wait();

            foreach (var message in newMessagesList)
            {
                Assert.IsTrue(message.ChatMessageID == 123);
            }

            foreach (var message in newMessagesList2)
            {
                Assert.IsTrue(message.ChatMessageID == 123);
            }
        }
    }
}

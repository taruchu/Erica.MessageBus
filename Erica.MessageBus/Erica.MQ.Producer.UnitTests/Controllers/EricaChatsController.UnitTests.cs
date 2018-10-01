using Erica.MQ.Producer.UnitTests.Helpers;
using EricaChats.DataAccess.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharedInterfaces.Interfaces.EricaChats;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Erica.MQ.Producer.UnitTests
{
    [TestClass]
    public class EricaChatsControllerUnitTests
    {
        private HttpResponseMessage SendRequest(string jsonEricaChatsMessage, bool put = false)
        {
            var client = new HttpClient();
            HttpResponseMessage mqProducerResponse = null;
            var content = new StringContent(jsonEricaChatsMessage, Encoding.UTF8, "application/json");
            Task<HttpResponseMessage> mqTask = (put == false) ? client.PostAsync("http://localhost:50000/api/ericachats", content) :
                client.PutAsync("http://localhost:50000/api/ericachats", content);

            mqTask.Wait();
            switch (mqTask.Status)
            {
                case TaskStatus.Faulted:
                    throw new ApplicationException(mqTask.Exception.Flatten().InnerException.Message, mqTask.Exception.Flatten().InnerException);
                case TaskStatus.RanToCompletion:
                    mqProducerResponse = mqTask.Result;
                    break;
            }
            return mqProducerResponse;
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
            IEricaChats_MessageDTO mqProducerResponse = JsonMarshaller.UnMarshall<EricaChats_MessageDTO>(contentBody);
            Assert.IsNotNull(mqProducerResponse);

            string newSenderUserName = "God is Jesus Christ";
            mqProducerResponse.SenderUserName = newSenderUserName;

            string mqUpdatedRequest = JsonMarshaller.Marshall(mqProducerResponse);
            HttpResponseMessage updatedResponse = SendRequest(mqUpdatedRequest, true);

            Assert.IsTrue(updatedResponse.IsSuccessStatusCode);

            string updatedContentBody = string.Empty;
            Task<string> updateTask = updatedResponse.Content.ReadAsStringAsync();
            updateTask.Wait();
            switch (contentTask.Status)
            {
                case TaskStatus.Faulted:
                    throw new ApplicationException(updateTask.Exception.Flatten().InnerException.Message, updateTask.Exception.Flatten().InnerException);
                case TaskStatus.RanToCompletion:
                    updatedContentBody = updateTask.Result;
                    break;
            }

            Assert.IsFalse(String.IsNullOrEmpty(updatedContentBody));
            IEricaChats_MessageDTO updatedMqProducerResponse = JsonMarshaller.UnMarshall<EricaChats_MessageDTO>(updatedContentBody);
            Assert.AreEqual(newSenderUserName, updatedMqProducerResponse.SenderUserName);
            Assert.IsTrue(String.IsNullOrEmpty(updatedMqProducerResponse.ErrorMessage));
        }
    }
}

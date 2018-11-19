using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharedInterfaces.Helpers;
using SharedInterfaces.Interfaces.EricaChats;
using SharedInterfaces.Models.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.UnitTests
{
    [TestClass]
    public class FileManagerUnitTests
    { 
        [TestMethod]
        public void TestUploadAndDownloadFile()
        {
            IEricaChats_FileDTO ericaChats_FileDTO = new EricaChats_FileDTO();

            //************************ Test Upload
            //Set up the request File DTO and it's contents
            byte[] fileBytes = GetEmbeddedResourceAsBytes("TestFiles/TextFile.txt");
            string fileBytesAsBase64String = GetBase64StringFromBytes(fileBytes);
            ericaChats_FileDTO.FileBytesAsAsBase64String = fileBytesAsBase64String;
            ericaChats_FileDTO.FileNameGuid = Guid.NewGuid().ToString();
            string payload = JsonMarshaller.Marshall(ericaChats_FileDTO);

            //Call the File Manager to perform an upload
            HttpResponseMessage uploadResponse = GetFileManagerResponse(payload, "UploadFile", true);
            Assert.IsNotNull(uploadResponse);
            Assert.IsTrue(uploadResponse.IsSuccessStatusCode);
            

            //************************ Test Download
            //Now use the File Guid to Download the file using the File Manager
            string endpoint = $"DownloadFile/{ericaChats_FileDTO.FileNameGuid}";
            HttpResponseMessage response = GetFileManagerResponse(string.Empty, endpoint);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccessStatusCode);

            //Get the content body from the response
            string contentBody = GetResponseContentBody(response);
            Assert.IsFalse(String.IsNullOrEmpty(contentBody));

            //Compare the response File DTO contents to the request File DTO contents. They should be identical.
            IEricaChats_FileDTO ericaChats_FileDTO_Downloaded = JsonMarshaller.UnMarshall<EricaChats_FileDTO>(contentBody);
            Assert.IsTrue(String.Compare(ericaChats_FileDTO.FileNameGuid, ericaChats_FileDTO_Downloaded.FileNameGuid, StringComparison.CurrentCulture) == 0);
            Assert.IsTrue(String.Compare(ericaChats_FileDTO.FileBytesAsAsBase64String, ericaChats_FileDTO_Downloaded.FileBytesAsAsBase64String, StringComparison.CurrentCulture) == 0);  
        }

        [TestMethod]
        public void TestGetFileMetaDataByChannel()
        {
            //SetUp: Create some file meta data in the EricaChats Database and make sure each chat message has ChannelID = 1.

            //Call the File Manager giving it a valid ChannelId
            string endpoint = $"GetFileMetaDataByChannel/1"; 
            HttpResponseMessage metaResponse = GetFileManagerResponse(string.Empty, endpoint);
            Assert.IsNotNull(metaResponse);

            //Get the content body from the response
            string contentBody = GetResponseContentBody(metaResponse);             

            //Check the response list and UnMarshall it into a List of File DTO objects.
            Assert.IsFalse(String.IsNullOrEmpty(contentBody));
            List<IEricaChats_MessageDTO> responseList = JsonMarshaller.UnMarshall<List<IEricaChats_MessageDTO>>(contentBody);

            //Verify the list
            foreach(var fileMetaDTO in responseList)
            {
                Assert.IsFalse(String.IsNullOrEmpty(fileMetaDTO.FileAttachmentGUID));
                Assert.IsFalse(String.IsNullOrEmpty(fileMetaDTO.FriendlyFileName));
            }
        }

        private HttpResponseMessage GetFileManagerResponse(string payload, string endpoint, bool post = false)
        {
            Task<HttpResponseMessage> fileManagerTask = SendRequestToFileManager(payload, endpoint, post);
            fileManagerTask.Wait();
            HttpResponseMessage response = null;
            switch (fileManagerTask.Status)
            {
                case TaskStatus.Faulted:
                    throw new ApplicationException(fileManagerTask.Exception.Flatten().InnerException.Message, fileManagerTask.Exception.Flatten().InnerException);
                case TaskStatus.RanToCompletion:
                    response = fileManagerTask.Result;
                    break;
            }
            return response;
        }

        private async Task<HttpResponseMessage> SendRequestToFileManager(string payload, string endpoint, bool post = false)
        {
            var client = new HttpClient();
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            string url = $"http://localhost:50001/api/EricaChatsFiles/"+ endpoint;
            HttpResponseMessage response = null;
            if (post)
                response = await client.PostAsync(url, content);
            else
                response = await client.GetAsync(url);

            response.EnsureSuccessStatusCode();
            return response;
        }

        private string GetResponseContentBody(HttpResponseMessage response)
        {
            Assert.IsNotNull(response);
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
            return contentBody;
        }

        private string FormatResourceName(Assembly assembly, string resourceName)
        {
            string formattedResourceName = resourceName.Replace(" ", "_")
                                                       .Replace("\\", ".")
                                                       .Replace("/", ".");
            return $"{assembly.GetName().Name}.{formattedResourceName}";
        }

        private byte[] GetEmbeddedResourceAsBytes(string resourceName)
        {
            Assembly assembly = Assembly.GetCallingAssembly();
            string formattedResourceName = FormatResourceName(assembly, resourceName);
            using (Stream resourceStream = assembly.GetManifestResourceStream(formattedResourceName))
            {
                byte[] fileBytes = new byte[resourceStream.Length];
                resourceStream.Read(fileBytes, 0, fileBytes.Length);
                return fileBytes;                
            }
        }

        private string GetBase64StringFromBytes(byte[] fileBytes)
        {
            return Convert.ToBase64String(fileBytes);
        } 
    }
}

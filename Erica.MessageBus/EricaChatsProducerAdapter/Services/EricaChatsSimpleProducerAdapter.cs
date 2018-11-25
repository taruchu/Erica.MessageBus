using EricaChats.DataAccess.Services.SQL;
using SharedInterfaces.Helpers;
using SharedInterfaces.Models.EricaMQ;
using SharedInterfaces.Constants.EricaChats;
using SharedInterfaces.Interfaces.DataTransferObjects;
using SharedInterfaces.Interfaces.EricaChats;
using SharedInterfaces.Models.EricaChatsFiles;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EricaChats.ProducerAdapter.Services
{
    public class EricaChatsSimpleProducerAdapter : IEricaChatsSimpleProducerAdapter
    {  
        /*
         * This Adapter is a simple example. It will only perform a POST or PUT on 
         * the database for the chat message parameter passed in. Another more complex
         * adapter can be created for processing complex rules and performing multiple
         * database CRUD operations. Maybe even forwarding information to some remote
         * RESTful service. 
         *  
         */

        private EricaChats_DBContext _ericaChats_DBContext { get; set; }
        private IHttpClientFactory _httpClientFactory { get; set; }

        public EricaChatsSimpleProducerAdapter(EricaChats_DBContext ericaChats_DBContext, IHttpClientFactory httpClientFactory)
        {
            _ericaChats_DBContext = ericaChats_DBContext;
        }

        public async Task<IEricaMQ_MessageDTO> Produce(object request)
        {
            try
            {
                //TODO: Remmber to add a filter here so I don't process wrong messages.
                IEricaChats_MessageDTO ericaMessageRequest = (IEricaChats_MessageDTO)request;
                IEricaChats_MessageDTO ericaMessageProcessed;
                IEricaMQ_MessageDTO mqMessage = new EricaMQ_Message();

                if (String.IsNullOrEmpty(ericaMessageRequest.FileAttachmentGUID) == false &&
                     String.IsNullOrEmpty(ericaMessageRequest.FriendlyFileName) == false &&
                     String.IsNullOrEmpty(ericaMessageRequest.FileBytesAsAsBase64String) == false)
                {
                    //NOTE: If there is a file attachement upload it to the File Manager, all updates will cause original to be versioned.
                    await UploadFileToFileManager(ericaMessageRequest);
                }

                if (ericaMessageRequest.ChatMessageID > 0)
                { 
                    ericaMessageProcessed = _ericaChats_DBContext.PUT(ericaMessageRequest);

                    mqMessage.Context = EricaChatsConstants.Context_UpdateChatMessage;
                    mqMessage.Sender = ericaMessageProcessed.SenderUserName;
                    mqMessage.Data = JsonMarshaller.Marshall(ericaMessageProcessed);
                    mqMessage.AdapterAssemblyQualifiedName = typeof(IEricaChatsSimpleConsumerAdapter).AssemblyQualifiedName;
                }
                else
                { 
                    ericaMessageProcessed = _ericaChats_DBContext.POST(ericaMessageRequest);

                    mqMessage.Context = EricaChatsConstants.Context_CreateChatMessage;
                    mqMessage.Sender = ericaMessageProcessed.SenderUserName;
                    mqMessage.Data = JsonMarshaller.Marshall(ericaMessageProcessed);
                    mqMessage.AdapterAssemblyQualifiedName = typeof(IEricaChatsSimpleConsumerAdapter).AssemblyQualifiedName;
                }
                return mqMessage;
            }
            catch(Exception ex)
            {
                throw new ApplicationException(ex.Message, ex);
            }
        }

        private async Task UploadFileToFileManager(IEricaChats_MessageDTO ericaMessageRequest)
        { 
            IEricaChats_FileDTO ericaChats_FileDTO = new EricaChats_FileDTO();
            ericaChats_FileDTO.FileNameGuid = ericaMessageRequest.FileAttachmentGUID;
            ericaChats_FileDTO.FileBytesAsAsBase64String = ericaMessageRequest.FileBytesAsAsBase64String;
            string ericaChats_FileDTO_Json = JsonMarshaller.Marshall(ericaChats_FileDTO);

            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(ericaChats_FileDTO_Json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync("http://localhost:50002/api/EricaChatsFiles/UploadFile", content);
            response.EnsureSuccessStatusCode(); 
        }
    }
}

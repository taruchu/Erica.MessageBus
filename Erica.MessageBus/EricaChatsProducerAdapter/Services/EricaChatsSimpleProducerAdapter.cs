using EricaChats.DataAccess.Models;
using EricaChats.DataAccess.Services.SQL;
using EricaChats.ProducerAdapter.Helpers;
using EricaChats.ProducerAdapter.Models;
using SharedInterfaces.Interfaces.DataTransferObjects;
using SharedInterfaces.Interfaces.EricaChats;
using System;

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

        public EricaChatsSimpleProducerAdapter(EricaChats_DBContext ericaChats_DBContext)
        {
            _ericaChats_DBContext = ericaChats_DBContext;
        }

        public IEricaMQ_MessageDTO Produce(object request)
        {
            try
            {
                IEricaChats_MessageDTO ericaMessageRequest = (IEricaChats_MessageDTO)request;
                IEricaChats_MessageDTO ericaMessageProcessed;
                IEricaMQ_MessageDTO mqMessage = new EricaMQ_Message();

                if (ericaMessageRequest.ChatMessageID > 0)
                {
                    ericaMessageProcessed = _ericaChats_DBContext.PUT(ericaMessageRequest); 

                    mqMessage.Context = "Update.ChatMessage";
                    mqMessage.Sender = ericaMessageProcessed.SenderUserName;
                    mqMessage.Data = JsonMarshaller.Marshall(ericaMessageProcessed); 
                }
                else
                {
                    ericaMessageProcessed = _ericaChats_DBContext.POST(ericaMessageRequest);

                    mqMessage.Context = "Create.ChatMessage";
                    mqMessage.Sender = ericaMessageProcessed.SenderUserName;
                    mqMessage.Data = JsonMarshaller.Marshall(ericaMessageProcessed);
                }
                return mqMessage;
            }
            catch(Exception ex)
            {
                throw new ApplicationException(ex.Message, ex);
            }
        }
    }
}

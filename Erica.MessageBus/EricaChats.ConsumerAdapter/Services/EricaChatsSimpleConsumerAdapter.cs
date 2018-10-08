using EricaChats.ConsumerAdapter.Helpers;
using EricaChats.DataAccess.Models;
using SharedInterfaces.Interfaces.DataTransferObjects;
using SharedInterfaces.Interfaces.EricaChats;
using System;

namespace EricaChats.ConsumerAdapter
{
    public class EricaChatsSimpleConsumerAdapter : IEricaChatsSimpleConsumerAdapter
    {

        public EricaChatsSimpleConsumerAdapter()
        {

        }

        public object Consume(IEricaMQ_MessageDTO message)
        {
            //TODO: Add some filtering ???

            IEricaChats_MessageDTO ericaChatsMessage = JsonMarshaller.UnMarshall<EricaChats_MessageDTO>(message.Data);
            ericaChatsMessage.ChatMessageBody += " Consumed"; //NOTE: Just for testing
            //TODO: Add a Slack integration here :=)
            return ericaChatsMessage;
        }
    }
}

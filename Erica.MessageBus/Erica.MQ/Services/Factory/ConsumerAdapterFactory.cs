using Erica.MQ.Interfaces.Factory;
using EricaChats.ConsumerAdapter;
using EricaMQ.Helpers;
using SharedInterfaces.Interfaces.Adapters;
using SharedInterfaces.Interfaces.DataTransferObjects;
using SharedInterfaces.Interfaces.EricaChats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Erica.MQ.Services.Factory
{
    public class ConsumerAdapterFactory : IConsumerAdapterFactory
    {
        public ConsumerAdapterFactory()
        {
        }

        public string Consume(IEricaMQ_MessageDTO message)
        {
            try
            { 
                string consumedMessage = string.Empty;

                if (message.AdapterAssemblyQualifiedName == typeof(IEricaChatsSimpleConsumerAdapter).ToString())
                {
                    IConsumerAdapter adapter = new EricaChatsSimpleConsumerAdapter();
                    IEricaChats_MessageDTO ericaChats_MessageDTO = (IEricaChats_MessageDTO)adapter.Consume(message);
                    consumedMessage = JsonMarshaller.Marshall(ericaChats_MessageDTO);
                } 
                return consumedMessage; //TODO: Log this miss :=)
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message, ex);
            }
        }
    }
}

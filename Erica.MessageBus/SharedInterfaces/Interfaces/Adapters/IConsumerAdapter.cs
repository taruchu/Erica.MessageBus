using SharedInterfaces.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharedInterfaces.Interfaces.Adapters
{
    public interface IConsumerAdapter
    {
        /*
         * The basic purpose of this Adapter is to translate the 
         * EricaMQ message that is pulled off the message bus into a format that the extenal client
         * can understand/consume.  
         * 
         * The Adapter can also be programmed to perform pre-processing,
         * rule validation, and or maybe calculations before handing over the 
         * final "client formatted" message.
         */
        object Consume(IEricaMQ_MessageDTO message);
    }
}

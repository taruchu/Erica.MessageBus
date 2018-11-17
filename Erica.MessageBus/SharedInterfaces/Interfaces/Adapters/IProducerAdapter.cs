using SharedInterfaces.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SharedInterfaces.Interfaces.Adapters
{
    public interface IProducerAdapter
    {
        /*
         * The main purpose of this Adapter is to translate the 
         * request from the external client into a message format that EricaMQ 
         * will understand.  The Adapter can also be programmed to perform other 
         * operations like rule validation and or calculations before it hands over 
         * the final EricaMQ message data transfer object (DTO).
         */
        Task<IEricaMQ_MessageDTO> Produce(object request);
    }
}

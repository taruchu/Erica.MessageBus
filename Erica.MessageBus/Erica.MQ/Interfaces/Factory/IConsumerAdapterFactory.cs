using SharedInterfaces.Interfaces.Adapters;
using SharedInterfaces.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Erica.MQ.Interfaces.Factory
{
    public interface IConsumerAdapterFactory
    {
        string Consume(IEricaMQ_MessageDTO message);
    }
}

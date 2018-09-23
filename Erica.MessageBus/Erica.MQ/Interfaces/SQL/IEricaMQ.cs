using Erica.MQ.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Erica.MQ.Interfaces.SQL
{
    public interface IEricaMQ
    {
        string POST(IEricaMQ_MessageDTO jsonMessage);
        string PUT(IEricaMQ_MessageDTO jsonMessage);
        List<IEricaMQ_MessageDTO> GET(DateTime afterThisTimeStamp, int maxAmount);
    }
}

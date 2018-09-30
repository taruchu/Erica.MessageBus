using SharedInterfaces.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;

namespace Erica.MQ.Interfaces.SQL
{
    public interface IEricaMQ
    {
        IEricaMQ_MessageDTO POST(IEricaMQ_MessageDTO message);
        IEricaMQ_MessageDTO PUT(IEricaMQ_MessageDTO message);
        List<IEricaMQ_MessageDTO> GET(DateTime afterThisTimeStamp, int maxAmount);
    }
}

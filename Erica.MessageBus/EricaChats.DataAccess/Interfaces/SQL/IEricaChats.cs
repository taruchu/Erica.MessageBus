using SharedInterfaces.Interfaces.EricaChats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EricaChats.DataAccess.Interfaces.SQL
{
    public interface IEricaChats
    {
        IEricaChats_MessageDTO POST(IEricaChats_MessageDTO request);
        IEricaChats_MessageDTO PUT(IEricaChats_MessageDTO request);
        List<IEricaChats_MessageDTO> GetFileMetaDataList(int ChannelId);
    }
}

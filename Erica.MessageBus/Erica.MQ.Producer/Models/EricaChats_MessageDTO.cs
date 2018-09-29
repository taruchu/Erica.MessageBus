using SharedInterfaces.Interfaces.EricaChats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Erica.MQ.Producer.Models
{
    public class EricaChats_MessageDTO : IEricaChats_MessageDTO
    {
        public long ChatMessageID { get; set; }
        public long ChatChannelID { get; set; }
        public string ChatChannelName { get; set; }
        public string SenderUserName { get; set; }
        public string ChatMessageBody { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime ModifiedDateTime { get; set; }
        public string ErrorMessage { get; set; }
    }
}

using SharedInterfaces.Interfaces.EricaChats;
using System;

namespace EricaChats.DataAccess.Models
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

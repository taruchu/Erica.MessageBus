using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EricaChats.DataAccess.Models
{
    public class ChatMessage
    {
        [Key]
        public long ChatMessageID { get; set; }

        [ForeignKey("ChannelID")]
        public long ChannelID { get; set; }

        public Channel Channel { get; set; }
        public string SenderUserName { get; set; }
        public string ChatMessageBody { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime ModifiedDateTime { get; set; }
    }
}

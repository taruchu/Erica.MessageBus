using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EricaChats.DataAccess.Models
{
    public class Channel
    {
        [Key]
        public long ChannelID { get; set; }
        public string ChannelName { get; set; }

        public ICollection<ChatMessage> ChatMessages { get; set; }
    }
}

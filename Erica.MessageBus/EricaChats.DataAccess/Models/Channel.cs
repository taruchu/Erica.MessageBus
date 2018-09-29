using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Erica.MQ.Producer.Models
{
    public class Channel
    {
        [Key]
        public long ChannelID { get; set; }
        public string ChannelName { get; set; }

        public ICollection<ChatMessage> ChatMessages { get; set; }
    }
}

using SharedInterfaces.Interfaces.DataTransferObjects;
using System;
using System.ComponentModel.DataAnnotations;

namespace SharedInterfaces.Models.EricaMQ
{
    public class EricaMQ_Message : IEricaMQ_MessageDTO
    {
        [Key]
        public long Id { get; set; }

        public string Sender { get; set; }
        public string Context { get; set; }
        public string FileAttachmentGUID { get; set; }
        public string Data { get; set; }

        public DateTime CreatedDateTime { get; set; }
        public DateTime ModifiedDateTime { get; set; }
        public string AdapterAssemblyQualifiedName { get; set; }
    }
}

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SharedInterfaces.Interfaces.DataTransferObjects
{
    public interface IEricaMQ_MessageDTO
    { 
        [Required]
        [DefaultValue(0)]
        long Id { get; set; }

        [Required]
        string Sender { get; set; }

        [Required]
        string Context { get; set; }

        string FileAttachmentGUID { get; set; }    

        //TODO: May change this to a list of adapters later
        string AdapterAssemblyQualifiedName { get; set; }

        [Required]
        string Data { get; set; }

        [DataType(DataType.DateTime)]
        DateTime CreatedDateTime { get; set; }

        [DataType(DataType.DateTime)]
        DateTime ModifiedDateTime { get; set; }
    }
}

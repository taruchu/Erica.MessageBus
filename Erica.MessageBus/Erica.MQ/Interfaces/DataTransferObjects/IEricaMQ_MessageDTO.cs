using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Erica.MQ.Interfaces.DataTransferObjects
{
    public interface IEricaMQ_MessageDTO
    { 
        //TODO: May not need this interface

        [Required]
        [DefaultValue(0)]
        long Id { get; set; }

        [Required]
        string Sender { get; set; }

        [Required]
        string Context { get; set; }

        string FileAttachmentGUID { get; set; }

        [Required]
        string Data { get; set; }

        [DataType(DataType.DateTime)]
        DateTime CreatedDateTime { get; set; }

        [DataType(DataType.DateTime)]
        DateTime ModifiedDateTime { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SharedInterfaces.Interfaces.EricaChats
{
    public interface IEricaChats_FileDTO 
    {
        [Required]
        string FileNameGuid { get; set; }

        [Required]
        string FileBytesAsAsBase64String { get; set; }
    }
}

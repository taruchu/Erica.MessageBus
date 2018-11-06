using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SharedInterfaces.Interfaces.EricaChats
{
    public interface IEricaChats_FileDTO : IEricaChats_FileMetaDataDTO
    { 
        [Required]
        string FileBytesAsAsString { get; set; }
    }
}

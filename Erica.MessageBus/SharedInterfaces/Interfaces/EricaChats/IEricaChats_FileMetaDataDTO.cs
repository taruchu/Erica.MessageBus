using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SharedInterfaces.Interfaces.EricaChats
{
    public interface IEricaChats_FileMetaDataDTO
    {
        [Required]
        string FileNameGuid { get; set; }

        //TODO: Meta Data ???
        [Required]
        string FriendlyName { get; set; }
    }
}

using SharedInterfaces.Interfaces.EricaChats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharedInterfaces.Models.DTO
{
    public class EricaChats_FileDTO : IEricaChats_FileDTO
    {
        public string FileNameGuid { get; set; }
        public string FileBytesAsAsBase64String { get; set; }
    }
}

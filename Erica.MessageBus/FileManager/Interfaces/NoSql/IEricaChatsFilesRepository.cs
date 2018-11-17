using SharedInterfaces.Interfaces.EricaChats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileManager.Interfaces.NoSql
{
    public interface IEricaChatsFilesRepository
    {
        Task<bool> UploadFileFromBytesAsync(string fileNameGUID, byte[] fileBytes);

        Task<byte[]> DownloadFileAsBytesAsync(string fileNameGUID);
       
    }
}

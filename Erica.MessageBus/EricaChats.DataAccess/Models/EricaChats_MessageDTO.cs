using SharedInterfaces.Interfaces.EricaChats;
using System;

namespace EricaChats.DataAccess.Models
{
    public class EricaChats_MessageDTO : IEricaChats_MessageDTO
    {
        //TODO: Add file attachment meta data and file bytes in string form, but only add a fileAttachmentGUID to the sql dbcontext object, I want to use NoSql
        //to store the files. The sql object will contain a primary key to access the file in the NoSql DB.
        public long ChatMessageID { get; set; }
        public long ChatChannelID { get; set; }
        public string ChatChannelName { get; set; }
        public string SenderUserName { get; set; }
        public string ChatMessageBody { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime ModifiedDateTime { get; set; }
        public string ErrorMessage { get; set; }
        public string FileAttachmentGUID { get; set; }
        public string FriendlyFileName { get; set; }
        public string FileBytesAsAsBase64String { get; set; }
    }
}
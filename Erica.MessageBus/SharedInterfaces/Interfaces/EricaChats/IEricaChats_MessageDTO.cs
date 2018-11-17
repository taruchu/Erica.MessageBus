using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SharedInterfaces.Interfaces.EricaChats
{
    public interface IEricaChats_MessageDTO
    {
        [Required]
        [DefaultValue(0)]
        long ChatMessageID { get; set; }

        [Required]
        [DefaultValue(0)]
        long ChatChannelID { get; set; }

        [Required]
        string ChatChannelName { get; set; }

        [Required]
        string SenderUserName { get; set; }

        [Required]
        string ChatMessageBody { get; set; }

        [DataType(DataType.DateTime)]
        DateTime CreatedDateTime { get; set; }

        [DataType(DataType.DateTime)]
        DateTime ModifiedDateTime { get; set; }

        string ErrorMessage { get; set; }

        string FileAttachmentGUID { get; set; } 
        string FriendlyFileName { get; set; }
        string FileBytesAsAsBase64String { get; set; }
    }
}

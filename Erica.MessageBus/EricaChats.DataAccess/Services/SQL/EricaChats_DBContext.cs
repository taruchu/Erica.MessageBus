﻿using EricaChats.DataAccess.Interfaces.SQL;
using EricaChats.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using SharedInterfaces.Interfaces.EricaChats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EricaChats.DataAccess.Services.SQL
{
    public class EricaChats_DBContext : DbContext, IEricaChats
    {
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Channel> Channels { get; set; }

        public EricaChats_DBContext(DbContextOptions<EricaChats_DBContext> options) : base(options)
        {  
        }

        public IEricaChats_MessageDTO SetErrorMessageForInvalidChatChannel(IEricaChats_MessageDTO invalidRequest)
        {
            invalidRequest.ErrorMessage += String.Format("The chat channel name {0} that you provided does not exists. ", invalidRequest.ChatChannelName);
            invalidRequest.ErrorMessage += String.Format("The chat channel ID {0} that you provided does not exists. ", invalidRequest.ChatChannelID);
            return invalidRequest;
        }

        public IEricaChats_MessageDTO POST(IEricaChats_MessageDTO request)
        {
            using (var dbContextTransaction = this.Database.BeginTransaction())
            {
                try
                {
                    ChatMessage chatMessageInput = MaptoChatMessage(request);
                    if (chatMessageInput.Channel == null)
                    {
                        return SetErrorMessageForInvalidChatChannel(request);
                    }

                    chatMessageInput.CreatedDateTime = DateTime.Now;
                    this.ChatMessages.Add(chatMessageInput);
                    this.SaveChanges();
                    dbContextTransaction.Commit();
                    IEricaChats_MessageDTO recipt = MapToDTO(chatMessageInput);
                    return recipt;
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    throw new ApplicationException(ex.Message, ex);
                } 
            }
        }

        public IEricaChats_MessageDTO PUT(IEricaChats_MessageDTO request)
        {
            using (var dbContextTransaction = this.Database.BeginTransaction())
            {
                try
                {
                    ChatMessage chatMessageInput = MaptoChatMessage(request);
                    if(chatMessageInput.Channel == null)
                    {
                        return SetErrorMessageForInvalidChatChannel(request);
                    }

                    chatMessageInput.ModifiedDateTime = DateTime.Now;
                    ChatMessage updatedChatMessage = this.ChatMessages.Find(chatMessageInput.ChatMessageID);
                    //NOTE: Ensure the CreatedDateTime isn't modified by the client.
                    chatMessageInput.CreatedDateTime = updatedChatMessage.CreatedDateTime;
                    this.Entry(updatedChatMessage).CurrentValues.SetValues(chatMessageInput);
                    this.SaveChanges();
                    dbContextTransaction.Commit();
                    IEricaChats_MessageDTO recipt = MapToDTO(updatedChatMessage);
                    return recipt;
                }
                catch(Exception ex)
                {
                    dbContextTransaction.Rollback();
                    throw new ApplicationException(ex.Message, ex);
                }
            }
        }

        public IEricaChats_MessageDTO MapToDTO(ChatMessage chatMessage)
        {
            try
            {
                IEricaChats_MessageDTO ericaChats_MessageDTO = new EricaChats_MessageDTO();
                ericaChats_MessageDTO.ChatChannelID = chatMessage.ChannelID;
                ericaChats_MessageDTO.ChatChannelName = chatMessage.Channel.ChannelName;
                ericaChats_MessageDTO.ChatMessageID = chatMessage.ChatMessageID;
                ericaChats_MessageDTO.ChatMessageBody = chatMessage.ChatMessageBody;
                ericaChats_MessageDTO.CreatedDateTime = chatMessage.CreatedDateTime;
                ericaChats_MessageDTO.ModifiedDateTime = chatMessage.ModifiedDateTime;
                ericaChats_MessageDTO.SenderUserName = chatMessage.SenderUserName;
                ericaChats_MessageDTO.FileAttachmentGUID = chatMessage.FileAttachmentGUID;
                ericaChats_MessageDTO.FriendlyFileName = chatMessage.FriendlyFileName;
                return ericaChats_MessageDTO;
            }
            catch(Exception ex)
            {
                throw new ApplicationException(ex.Message, ex);
            }
        }

        public ChatMessage MaptoChatMessage(IEricaChats_MessageDTO request)
        {
            try
            {
                ChatMessage chatMessage = new ChatMessage();
                chatMessage.Channel = MapToChannel(request);
                chatMessage.ChannelID = chatMessage.Channel.ChannelID;
                chatMessage.ChatMessageID = request.ChatMessageID;
                chatMessage.ChatMessageBody = request.ChatMessageBody;
                chatMessage.SenderUserName = request.SenderUserName;
                chatMessage.FileAttachmentGUID = request.FileAttachmentGUID;
                chatMessage.FriendlyFileName = request.FriendlyFileName;
                return chatMessage;
            }
            catch(Exception ex)
            {
                throw new ApplicationException(ex.Message, ex);
            }
        }

        public Channel MapToChannel(IEricaChats_MessageDTO request)
        {
            try
            {
                Channel channel = new Channel();
                if (request.ChatChannelID > 0)
                    channel = this.Channels.Find(request.ChatChannelID);
                else if (String.IsNullOrEmpty(request.ChatChannelName) == false)
                    channel = this.Channels.FirstOrDefault(chnl => chnl.ChannelName == request.ChatChannelName);
                else
                    channel = null;
                return channel;
            }
            catch(Exception ex)
            {
                throw new ApplicationException(ex.Message, ex);
            }
        }

        public List<IEricaChats_MessageDTO> GetFileMetaDataList(int ChannelId)
        {
            using (var dbContextTransaction = this.Database.BeginTransaction())
            {
                try
                {
                    List<IEricaChats_MessageDTO> results = this.ChatMessages
                                                          .Where(msg => msg.ChannelID == ChannelId && String.IsNullOrEmpty(msg.FileAttachmentGUID) == false)
                                                          .ToList()
                                                          .Select(
                                                                    (reference) =>
                                                                        {
                                                                            this.Entry(reference).Reference(chnl => chnl.Channel).Load();
                                                                            return reference;
                                                                        }
                                                                   )
                                                          .Select(chatmsg => MapToDTO(chatmsg))
                                                          .ToList();
                    dbContextTransaction.Commit();
                    return results;
                }
                catch(Exception ex)
                {
                    dbContextTransaction.Rollback();
                    throw new ApplicationException(ex.Message, ex);
                }
            }
        }
    }
}

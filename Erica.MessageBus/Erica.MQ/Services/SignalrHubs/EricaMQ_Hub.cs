﻿using Erica.MQ.Interfaces.Factory;
using Erica.MQ.Services.SQL;
using SharedInterfaces.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SharedInterfaces.Interfaces.DataTransferObjects;
using SharedInterfaces.Constants.EricaMQ_Hub;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Erica.MQ.Services.SignalrHubs
{
    [Authorize]
    public class EricaMQ_Hub : Hub
    {
        private EricaMQ_DBContext _ericaMQ_DBContext { get; set; } 
        private IConsumerAdapterFactory _consumerAdapterFactory { get; set; }
        private static ILogger _logger { get; set; }


        public EricaMQ_Hub(EricaMQ_DBContext ericaMQ_DBContext, IConsumerAdapterFactory consumerAdapterFactory, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(Assembly.GetExecutingAssembly().FullName);
            _ericaMQ_DBContext = ericaMQ_DBContext;
            _consumerAdapterFactory = consumerAdapterFactory; 
        } 

        private List<IEricaMQ_MessageDTO> GetMessageListInRange(DateTime afterThisTimeStamp, int maxAmount, DateTime beforeThisTimeStamp, string context = null)
        {
            var newMessages = (context == null) ? _ericaMQ_DBContext.GET(afterThisTimeStamp, maxAmount, beforeThisTimeStamp)
                            : _ericaMQ_DBContext.GetByContext(afterThisTimeStamp, maxAmount, beforeThisTimeStamp, context);
            return newMessages;
        } 
        
        public async Task<bool> GetMessagesInRangeBulkList(DateTime afterThisTimeStamp, int maxAmount, DateTime beforeThisTimeStamp)
        {
            try
            {
                var newMessages = GetMessageListInRange(afterThisTimeStamp, maxAmount, beforeThisTimeStamp);
                List<string> bulkList = new List<string>();
                int percentComplete = 0;
                foreach(var message in newMessages)
                {
                    string marshalledMessage = JsonMarshaller.Marshall(message);
                    bulkList.Add(marshalledMessage);
                    percentComplete = (bulkList.Count / 100) * newMessages.Count;
                    await Clients.Caller.SendAsync(Constants_EricaMQ_Hub.ClientEvent_ReceiveMessagesInRangeBulkListProcessingProgress, percentComplete);
                }

                await Clients.Caller.SendAsync(Constants_EricaMQ_Hub.ClientEvent_ReceiveMessagesInRangeBulkList, bulkList);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw new ApplicationException(ex.Message, ex);
            }
        }

        public async Task<bool> ConsumeMessagesInRangeBulkList(DateTime afterThisTimeStamp, int maxAmount, DateTime beforeThisTimeStamp)
        {
            try
            {
                var newMessages = GetMessageListInRange(afterThisTimeStamp, maxAmount, beforeThisTimeStamp);
                List<string> bulkList = new List<string>();
                int percentComplete = 0;
                int iterationCount = 0; 
                foreach (var message in newMessages)
                {
                    iterationCount++;
                    //NOTE: Messages must have an Adapter type defined in order to be consumed.
                    if (String.IsNullOrEmpty(message.AdapterAssemblyQualifiedName) == false)
                    {
                        string consumedMessage = _consumerAdapterFactory.Consume(message);
                        bulkList.Add(consumedMessage);
                        percentComplete = (iterationCount / 100) * newMessages.Count; //NOTE: If any messages are skipped, bulkList count will never equal newMessages count
                        await Clients.Caller.SendAsync(Constants_EricaMQ_Hub.ClientEvent_ReceiveConsumedMessagesInRangeBulkListProcessingProgress, percentComplete);
                    } 
                }

                await Clients.Caller.SendAsync(Constants_EricaMQ_Hub.ClientEvent_ReceiveConsumedMessagesInRangeBulkList, bulkList);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw new ApplicationException(ex.Message, ex);
            }
        }

        public async Task<string> GetMessagesInRange(DateTime afterThisTimeStamp, int maxAmount, DateTime beforeThisTimeStamp)
        {
            try
            {
                var newMessages = GetMessageListInRange(afterThisTimeStamp, maxAmount, beforeThisTimeStamp);
                string lastDateRead = string.Empty; 
                 
                foreach (var message in newMessages)
                {
                    await Clients.Caller.SendAsync(Constants_EricaMQ_Hub.ClientEvent_ReceiveMessagesInRange, JsonMarshaller.Marshall(message));
                    lastDateRead = message.CreatedDateTime.ToString("yyyy-MM-dd HH:mm:ss.fffffff"); 
                }
                return lastDateRead;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw new ApplicationException(ex.Message, ex);
            }
        }
         
        public async Task<string> ConsumeMessagesInRange(DateTime afterThisTimeStamp, int maxAmount, DateTime beforeThisTimeStamp)
        {
            try
            {                
                var newMessages = GetMessageListInRange(afterThisTimeStamp, maxAmount, beforeThisTimeStamp);
                string lastDateRead = string.Empty;

                foreach (var message in newMessages)
                {    
                    //NOTE: Messages must have an Adapter type defined in order to be consumed.
                    if (String.IsNullOrEmpty(message.AdapterAssemblyQualifiedName) == false) 
                    {
                        string consumedMessage = _consumerAdapterFactory.Consume(message);
                        await Clients.Caller.SendAsync(Constants_EricaMQ_Hub.ClientEvent_ReceiveConsumedMessagesInRange, consumedMessage);
                    }

                    lastDateRead = message.CreatedDateTime.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
                }
                return lastDateRead;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw new ApplicationException(ex.Message, ex);
            }
        } 

        public async Task<bool> SubscribeToLatestMessage()
        {
            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, Constants_EricaMQ_Hub.GroupName_LatestMessage); 
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw new ApplicationException(ex.Message, ex);
            }
        }

        public async Task<bool> UnSubscribeToLatestMessage()
        {
            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, Constants_EricaMQ_Hub.GroupName_LatestMessage);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw new ApplicationException(ex.Message, ex);
            }
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw new ApplicationException(ex.Message, ex);
            }
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            try
            {
                await base.OnDisconnectedAsync(ex);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, Constants_EricaMQ_Hub.GroupName_LatestMessage);
            }
            catch (Exception e)
            {
                _logger.LogError(ex, ex.Message);
                throw new ApplicationException(e.Message, e);
            }
        }
    }
}

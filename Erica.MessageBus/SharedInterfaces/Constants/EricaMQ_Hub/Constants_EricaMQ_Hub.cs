using System;
using System.Collections.Generic;
using System.Text;

namespace SharedInterfaces.Constants.EricaMQ_Hub
{
    public class Constants_EricaMQ_Hub
    {
        //Groups
        public static string GroupName_LatestMessage = "GroupName_LatestMessage";

        //Client Events
        public static string ClientEvent_ReceiveLatestMessage = "ClientEvent_ReceiveLatestMessage";
        public static string ClientEvent_ReceiveLatestConsumedMessage = "ClientEvent_ReceiveLatestConsumedMessage";
        public static string ClientEvent_ReceiveConsumedMessagesInRange = "ClientEvent_ReceiveConsumedMessagesInRange";
        public static string ClientEvent_ReceiveMessagesInRange = "ClientEvent_ReceiveMessagesInRange";
        public static string ClientEvent_ReceiveMessagesInRangeBulkListProcessingProgress = "ClientEvent_ReceiveMessagesInRangeBulkListProcessingProgress";
        public static string ClientEvent_ReceiveMessagesInRangeBulkList = "ClientEvent_ReceiveMessagesInRangeBulkList";
        public static string ClientEvent_ReceiveConsumedMessagesInRangeBulkListProcessingProgress = "ClientEvent_ReceiveConsumedMessagesInRangeBulkListProcessingProgress";
        public static string ClientEvent_ReceiveConsumedMessagesInRangeBulkList = "ClientEvent_ReceiveConsumedMessagesInRangeBulkList";


        //Hub Methods
        public static string HubMethod_SubscribeToLatestMessage = "SubscribeToLatestMessage";
        public static string HubMethod_UnSubscribeToLatestMessage = "UnSubscribeToLatestMessage";
        public static string HubMethod_GetMessagesInRangeBulkList = "GetMessagesInRangeBulkList";
        public static string HubMethod_ConsumeMessagesInRangeBulkList = "ConsumeMessagesInRangeBulkList";
        public static string HubMethod_GetMessagesInRange = "GetMessagesInRange";
        public static string HubMethod_ConsumeMessagesInRange = "ConsumeMessagesInRange"; 
    }
}

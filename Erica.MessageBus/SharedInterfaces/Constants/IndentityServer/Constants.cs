using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharedInterfaces.Constants.IdentityServer
{
    public class Constants
    {
        //URLs
        public static string IdentityServerUrl = "http://localhost:50001";

        //Resources or Scopes
        public static string EricaMQ_Api = "EricaMQ";
        public static string EricaMQ_ApiDisplayName = "Erica Message Queue";
        public static string EricaMQProducer_Api = "EricaMQ_Producer";
        public static string EricaMQProducer_ApiDisplayName = "Erica Message Queue Producer";
        public static string EricaMQConsumer_Api = "EricaMQ_Consumer";
        public static string EricaMQConsumer_ApiDisplayName = "Erica Message Queue Consumer";


        //Clients
        public static string EricaMQProducer_Client = "EricaMQProducer_Client";
        public static string EricaMQConsumer_Client = "EricaMQConsumer_Client";
        public static string ExternalClient = "ExternalClient";

        //Secrets will try to use environment variables first. 
        public static string EricaMQProducer_ClientSecret = String.IsNullOrEmpty(Environment.GetEnvironmentVariable("ERICAMQPRODUCER_CLIENTSECRET")) ? "God" 
            : Environment.GetEnvironmentVariable("ERICAMQPRODUCER_CLIENTSECRET");
        public static string EricaMQConsumer_ClientSecret = String.IsNullOrEmpty(Environment.GetEnvironmentVariable("ERICAMQCONSUMER_CLIENTSECRET")) ? "Jesus"
            : Environment.GetEnvironmentVariable("ERICAMQCONSUMER_CLIENTSECRET");
        public static string ExternalClient_ClientSecret = String.IsNullOrEmpty(Environment.GetEnvironmentVariable("EXTERNALCLIENT_CLIENTSECRET")) ? "Hosana"
            : Environment.GetEnvironmentVariable("EXTERNALCLIENT_CLIENTSECRET");

        //Schemes
        public static string Bearer = "Bearer";
    }
}

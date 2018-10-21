using IdentityServer4.Models;
using SharedInterfaces.Constants.IdentityServer;
using System.Collections.Generic;

namespace IdentityServer.IdentityServerConfig
{
    public class Config
    {
        public Config() { }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource(Constants_IdentityServer.EricaMQ_Api, Constants_IdentityServer.EricaMQ_ApiDisplayName),
                new ApiResource(Constants_IdentityServer.EricaMQProducer_Api, Constants_IdentityServer.EricaMQProducer_ApiDisplayName),
                new ApiResource(Constants_IdentityServer.EricaMQConsumer_Api, Constants_IdentityServer.EricaMQConsumer_ApiDisplayName)
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = Constants_IdentityServer.EricaMQProducer_Client,
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets =
                    {
                        new Secret(Constants_IdentityServer.EricaMQProducer_ClientSecret.Sha256()) 
                    },
                    AllowedScopes = {Constants_IdentityServer.EricaMQ_Api}                    
                },
                new Client
                {
                    ClientId = Constants_IdentityServer.EricaMQConsumer_Client,
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets =
                    {
                        new Secret(Constants_IdentityServer.EricaMQConsumer_ClientSecret.Sha256())
                    },
                    AllowedScopes = {Constants_IdentityServer.EricaMQ_Api}
                },
                new Client
                {
                    ClientId = Constants_IdentityServer.ExternalClient,
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets =
                    {
                        new Secret(Constants_IdentityServer.ExternalClient_ClientSecret.Sha256())
                    },
                    AllowedScopes = {Constants_IdentityServer.EricaMQProducer_Api, Constants_IdentityServer.EricaMQConsumer_Api}
                }
            };
        }
    }
}

using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer.IdentityServerConstants;

namespace IdentityServer.IdentityServerConfig
{
    public class Config
    {
        public Config() { }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource(Constants.EricaMQ_Api, Constants.EricaMQ_ApiDisplayName),
                new ApiResource(Constants.EricaMQProducer_Api, Constants.EricaMQProducer_ApiDisplayName),
                new ApiResource(Constants.EricaMQConsumer_Api, Constants.EricaMQConsumer_ApiDisplayName)
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = Constants.EricaMQProducer_Client,
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets =
                    {
                        new Secret(Constants.EricaMQProducer_ClientSecret.Sha256()) 
                    },
                    AllowedScopes = {Constants.EricaMQ_Api}                    
                },
                new Client
                {
                    ClientId = Constants.EricaMQConsumer_Client,
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets =
                    {
                        new Secret(Constants.EricaMQConsumer_ClientSecret.Sha256())
                    },
                    AllowedScopes = {Constants.EricaMQ_Api}
                },
                new Client
                {
                    ClientId = Constants.ExternalClient,
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets =
                    {
                        new Secret(Constants.ExternalClientSecret.Sha256())
                    },
                    AllowedScopes = {Constants.EricaMQProducer_Api, Constants.EricaMQConsumer_Api}
                }
            };
        }
    }
}

using IdentityServer4.Models;
using Microsoft.Extensions.Logging;
using SharedInterfaces.Constants.IdentityServer;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace IdentityServer.IdentityServerConfig
{
    public class Config
    {
        private static ILogger _logger { get; set; }
        public Config(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(Assembly.GetExecutingAssembly().FullName);
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            try
            {
                return new List<ApiResource>
            {
                new ApiResource(Constants_IdentityServer.EricaMQ_Api, Constants_IdentityServer.EricaMQ_ApiDisplayName),
                new ApiResource(Constants_IdentityServer.EricaMQProducer_Api, Constants_IdentityServer.EricaMQProducer_ApiDisplayName),
                new ApiResource(Constants_IdentityServer.EricaMQConsumer_Api, Constants_IdentityServer.EricaMQConsumer_ApiDisplayName)
            };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw new ApplicationException(ex.Message, ex);
            }
        }

        public static IEnumerable<Client> GetClients()
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw new ApplicationException(ex.Message, ex);
            }
        }
    }
}

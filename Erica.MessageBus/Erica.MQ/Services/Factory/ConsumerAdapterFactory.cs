using Erica.MQ.Interfaces.Factory;
using Erica.MQ.Services.IOC;
using EricaChats.ConsumerAdapter;
using EricaMQ.Helpers;
using Microsoft.Extensions.Logging;
using SharedInterfaces.Interfaces.Adapters;
using SharedInterfaces.Interfaces.DataTransferObjects;
using SharedInterfaces.Interfaces.EricaChats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Erica.MQ.Services.Factory
{
    public class ConsumerAdapterFactory : IConsumerAdapterFactory
    {
        private static ILogger _logger { get; set; }
        private UnityIOC _unityIOC { get; set; }
        public ConsumerAdapterFactory(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(Assembly.GetExecutingAssembly().FullName);
            _unityIOC = new UnityIOC();
        }

        public string Consume(IEricaMQ_MessageDTO message)
        {
            try
            { 
                string consumedMessage = string.Empty;

                if (String.IsNullOrEmpty(message.AdapterAssemblyQualifiedName) == false)
                { 
                    //Beautiful :=)
                    Type adapterType = Type.GetType(message.AdapterAssemblyQualifiedName);
                    MethodInfo method = typeof(UnityIOC).GetMethod("Resolve");
                    MethodInfo boundGenericMethod = method.MakeGenericMethod(adapterType);
                    IConsumerAdapter consumerAdapter = (IConsumerAdapter)boundGenericMethod.Invoke(_unityIOC, null);
                    consumedMessage = (string)consumerAdapter.Consume(message); 
                }  
                return consumedMessage; 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Whoa, error trying to resolve this adapter type: {message.AdapterAssemblyQualifiedName}");
                throw new ApplicationException(ex.Message, ex);
            }
        }
    }
}

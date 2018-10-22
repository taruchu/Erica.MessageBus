using EricaChats.ConsumerAdapter;
using SharedInterfaces.Interfaces.EricaChats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Unity;

namespace Erica.MQ.Services.IOC
{
    public class UnityIOC
    { 
        private UnityContainer _container { get; set; }
        public UnityIOC()
        {
            _container = new UnityContainer();
            Erect(_container);
        }

        private void Erect(UnityContainer container)
        {
            try
            {
                container
                        .RegisterType<IEricaChatsSimpleConsumerAdapter, EricaChatsSimpleConsumerAdapter>()

                    ;
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message, ex);
            }
        }

        public Type Resolve<Type>()
        {
            try
            { 
                return _container.Resolve<Type>();
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message, ex);
            }
        }
    }
}

using Newtonsoft.Json;
using System;

namespace SharedInterfaces.Helpers
{
    public class JsonMarshaller
    {
        public static string Marshall<U>(U data)
        {
            try
            {
                return JsonConvert.SerializeObject(data, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message, ex);
            }
        }

        public static U UnMarshall<U>(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<U>(json, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });
            }
            catch (Exception ex)
            { 
                throw new ApplicationException(ex.Message, ex);
            }
        }
    }
}

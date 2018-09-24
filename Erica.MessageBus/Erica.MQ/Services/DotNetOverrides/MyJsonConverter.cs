using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Erica.MQ.Services.DotNetOverrides
{
    public class MyJsonConverter<Tin, Tout> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(Tin));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize<Tout>(reader); 
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
           serializer.Serialize(writer, value);
        }
    }
}

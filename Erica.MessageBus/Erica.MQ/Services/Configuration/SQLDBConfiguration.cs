using Erica.MQ.Interfaces.Configuration;

namespace Erica.MQ.Services.Configuration
{
    public class SQLDBConfiguration : ISQLDBConfiguration
    {
        public SQLDBConfiguration()
        {
        }

        public string ConnectionString { get; set; }
    }
}

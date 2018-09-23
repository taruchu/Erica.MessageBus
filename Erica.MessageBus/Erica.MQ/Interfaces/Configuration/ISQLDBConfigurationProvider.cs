
namespace Erica.MQ.Interfaces.Configuration
{
    public interface ISQLDBConfigurationProvider
    {
        ISQLDBConfiguration GetSQLDBConfigurationFromJSONFile();
        ISQLDBConfiguration GetSQLDBConfigurationFromJSONString(string json);
    }
}

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Tinkoff.ISA.Infrastructure.Settings
{
    public static class SlackSerializerSettings
    {
        public static JsonSerializerSettings DefaultSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };
    }
}

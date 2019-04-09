using System.Linq;

namespace Tinkoff.ISA.Infrastructure.Configuration
{
    public class ConfigHelper
    {
        private static readonly string[] Environments =
        {
            AppEnvironment.Prod,
            AppEnvironment.DevDocker,
            AppEnvironment.Qa
        };

        public static string GetConfigByEnvironment(string envName)
        {
            return Environments.Contains(envName) ? $"appsettings.{envName}.json" : "appsettings.json";
        }
    }
}

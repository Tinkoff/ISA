using Microsoft.Extensions.DependencyInjection;
using Tinkoff.ISA.Infrastructure.MongoDb;

namespace Tinkoff.ISA.Infrastructure.Extensions
{   
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services)
        {
            services.AddSingleton<IMongoContext, MongoContext>();
            return services;
        }
    }
}

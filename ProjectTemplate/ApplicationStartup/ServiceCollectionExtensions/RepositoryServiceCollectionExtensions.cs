using Microsoft.Extensions.DependencyInjection;
using ProjectTemplate.Data.Repositories;

namespace ProjectTemplate.ApplicationStartup.ServiceCollectionExtensions
{
    public static class RepositoryServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositoryServices(this IServiceCollection services)
        {
            // Interface => concrete implementation
            services.AddScoped<UserRepository>();

            return services;
        }
    }
}
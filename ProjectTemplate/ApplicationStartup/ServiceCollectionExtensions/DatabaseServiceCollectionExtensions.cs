using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectTemplate.Data;

namespace ProjectTemplate.ApplicationStartup.ServiceCollectionExtensions
{
    public static class DatabaseServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<DataContext>(x => x.UseMySql(config.GetConnectionString("DefaultConnection"), options => options.EnableRetryOnFailure()));
            services.AddTransient<Seeder>();

            return services;
        }
    }
}
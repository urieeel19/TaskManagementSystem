using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Repositories;
using TaskManagement.Infrastructure.Security;

namespace TaskManagement.Infrastructure.DependencyInjection
{
    public static class InfrastructureServicesRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. MongoDB Native Connection Configuration
            var mongoConnectionString = configuration.GetConnectionString("MongoDb");
            var databaseName = configuration["ConnectionStrings:DatabaseName"];

            services.AddSingleton<IMongoClient>(sp => new MongoClient(mongoConnectionString));
            services.AddScoped(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(databaseName);
            });

            // 2. Repositories
            services.AddScoped<IUserRepository>(sp => new MongoUserRepository(sp.GetRequiredService<IMongoDatabase>()));
            services.AddScoped<ITaskRepository>(sp => new MongoTaskRepository(sp.GetRequiredService<IMongoDatabase>()));

            // 3. Security Services
            services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
            services.AddScoped<ITokenService, JwtTokenService>();

            // 4. Seeding Services
            services.AddScoped<DbInitializer>();

            return services;
        }
    }
}

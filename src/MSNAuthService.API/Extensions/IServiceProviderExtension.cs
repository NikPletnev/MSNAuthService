using MSNAuthService.Domain.Interfaces;
using MSNAuthService.Domain.Services;
using MSNAuthService.Infrastructure.Repositories;
using StackExchange.Redis;

namespace MSNAuthService.API.Extensions
{
    public static class IServiceProviderExtension
    {
        public static void RegisterProjectServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
        }

        public static void RegisterProjectRepositories(this IServiceCollection services)
        {
            services.AddScoped<ITokenRepository, RedisTokenRepository>();
        }

        public static void RegisterRedis(this IServiceCollection services, IConfiguration configuration)
        {
            var redisConnectionString = configuration["Redis:ConnectionString"];
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));
        }
    }
}

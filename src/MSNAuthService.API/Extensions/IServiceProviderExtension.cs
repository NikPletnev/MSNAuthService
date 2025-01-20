using Microsoft.EntityFrameworkCore;
using MSNAuthService.Domain.Interfaces;
using MSNAuthService.Domain.Services;
using MSNAuthService.Infrastructure;
using MSNAuthService.Infrastructure.Entities;
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
            services.AddScoped<IUserRepository, UserRepository>();
        }

        public static void RegisterRedis(this IServiceCollection services, IConfiguration configuration)
        {
            var redisConnectionString = configuration["Redis:ConnectionString"];
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));
        }

        public static void RegisterDb(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration["DbConnection:ConnectionString"];

            services.AddDbContext<AuthDbContext>(options =>
                options
                    .UseNpgsql(connectionString,
                        npgsqlOptions =>
                        {
                            npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(1),
                                errorCodesToAdd: null);
                            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory_AuthDb");
                            npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                        })
                        .EnableSensitiveDataLogging()
                        .EnableDetailedErrors(true),
                        ServiceLifetime.Scoped);
        }

        public static void SeedRoles(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<AuthDbContext>();
                if (!context.Roles.Any())
                {
                    context.Roles.AddRange(
                        new RoleEntity { Name = "Admin" },
                        new RoleEntity { Name = "User" }
                    );

                    context.SaveChanges();
                }
            }
        }
    }
}

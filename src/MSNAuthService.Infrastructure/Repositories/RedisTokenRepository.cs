using Microsoft.Extensions.Logging;
using MSNAuthService.Domain.Interfaces;
using MSNAuthService.Domain.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace MSNAuthService.Infrastructure.Repositories
{
    public class RedisTokenRepository : ITokenRepository
    {
        private readonly IDatabase _database;
        private readonly ILogger<RedisTokenRepository> _logger;

        public RedisTokenRepository(IConnectionMultiplexer redis, ILogger<RedisTokenRepository> logger)
        {
            _database = redis.GetDatabase();
            _logger = logger;
        }

        public async Task SaveRefreshTokenAsync(RefreshToken token)
        {
            var tokenJson = JsonSerializer.Serialize(token);
            _logger.LogInformation("Saving refresh token with ID: {TokenId}", token.Token);

            try
            {
                await _database.StringSetAsync(token.Token, tokenJson, TimeSpan.FromDays(7));
                _logger.LogInformation("Successfully saved refresh token with ID: {TokenId}", token.Token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving refresh token with ID: {TokenId}", token.Token);
                throw;
            }
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            _logger.LogInformation("Retrieving refresh token with ID: {TokenId}", token);

            try
            {
                var tokenJson = await _database.StringGetAsync(token);
                if (tokenJson.HasValue)
                {
                    _logger.LogInformation("Successfully retrieved refresh token with ID: {TokenId}", token);
                    return JsonSerializer.Deserialize<RefreshToken>(tokenJson!);
                }
                else
                {
                    _logger.LogWarning("Refresh token with ID: {TokenId} not found", token);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving refresh token with ID: {TokenId}", token);
                throw;
            }
        }

        public async Task RevokeRefreshTokenAsync(string token)
        {
            _logger.LogInformation("Revoking refresh token with ID: {TokenId}", token);

            try
            {
                await _database.KeyDeleteAsync(token);
                _logger.LogInformation("Successfully revoked refresh token with ID: {TokenId}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking refresh token with ID: {TokenId}", token);
                throw;
            }
        }
    }
}
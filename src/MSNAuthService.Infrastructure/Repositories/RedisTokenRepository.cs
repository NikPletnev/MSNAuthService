using MSNAuthService.Domain.Interfaces;
using MSNAuthService.Domain.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MSNAuthService.Infrastructure.Repositories
{
    public class RedisTokenRepository : ITokenRepository
    {
        private readonly IDatabase _database;

        public RedisTokenRepository(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public async Task SaveRefreshTokenAsync(RefreshToken token)
        {
            var tokenJson = JsonSerializer.Serialize(token);
            await _database.StringSetAsync(token.Token, tokenJson, TimeSpan.FromDays(7));
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            var tokenJson = await _database.StringGetAsync(token);
            return tokenJson.HasValue
                ? JsonSerializer.Deserialize<RefreshToken>(tokenJson!)
                : null;
        }

        public async Task RevokeRefreshTokenAsync(string token)
        {
            await _database.KeyDeleteAsync(token);
        }
    }
}

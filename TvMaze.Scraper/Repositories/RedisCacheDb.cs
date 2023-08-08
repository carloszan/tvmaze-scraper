using StackExchange.Redis;

namespace TvMaze.Scraper.Repositories
{
    public class RedisCacheDb : ICache
    {
        private readonly IDatabase _redisDb;

        public RedisCacheDb(IConnectionMultiplexer redisMultiplexer)
        {
            _redisDb = redisMultiplexer.GetDatabase();
        }

        public async Task<string> GetAsync(string key)
        {
            var value = await _redisDb.StringGetAsync(key);

            if (!value.HasValue) return "";

            return value.ToString();
        }
    }
}

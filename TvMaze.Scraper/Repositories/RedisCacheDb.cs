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

        public async Task SetAsync(string key, string value)
        {
            var success = await _redisDb.StringSetAsync(key, value);

            if (success)
                return;
            throw new Exception("Could not save to the database.");
        }
    }
}

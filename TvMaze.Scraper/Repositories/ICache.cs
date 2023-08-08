namespace TvMaze.Scraper.Repositories
{
    public interface ICache
    {
        public Task<string> GetAsync(string key);
    }
}

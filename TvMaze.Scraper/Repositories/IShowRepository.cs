using TvMaze.Scraper.Entities;

namespace TvMaze.Scraper.Repositories
{
    public interface IShowRepository
    {
        /// <summary>
        /// Upsert many shows at once. Also called batch insert. Async
        /// </summary>
        /// <param name="shows">List of shows that will be stored on database</param>
        /// <returns>Doesn't return anything but throws Exception if it couldn't save for any reason.</returns>
        public Task UpsertManyAsync(List<ShowEntity> shows);
    }
}

using TvMaze.Scraper.Services.Dtos;

namespace TvMaze.Scraper.Services
{
    public interface ITvMazeApi
    {
        /// <summary>
        /// Return all shows from TvMazeApi
        /// </summary>
        /// <param name="startPage">First page to look up</param>
        /// <param name="totalPages">Last page to look up</param>
        /// <param name="concurrentRequests">Concurrent requests</param>
        /// <param name="maxRequestsPerSecond">Max requests per second</param>
        /// <returns></returns>
        /// <exception cref="Exception">Exception is throw if any show was found</exception>
        Task<List<ShowDto>> GetShows(int startPage = 0, int totalPages = 0, int concurrentRequests = 1, int maxRequestsPerSecond = 10);

        Task<List<CastDto>> GetCast(int showId);
    }
}

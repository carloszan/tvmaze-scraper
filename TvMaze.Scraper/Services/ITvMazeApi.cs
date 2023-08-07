using TvMaze.Scraper.Services.Dtos;

namespace TvMaze.Scraper.Services
{
    public interface ITvMazeApi
    {
        Task<List<ShowDto>> GetShows(int startPage = 0, int totalPages = 0);
    }
}

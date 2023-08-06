namespace TvMaze.Scraper.Services
{
    public interface ITvMazeApi
    {
        Task<int> GetLastShowPage();
    }
}

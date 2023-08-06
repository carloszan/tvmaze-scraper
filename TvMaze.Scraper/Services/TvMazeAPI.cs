using System.Net.Http;

namespace TvMaze.Scraper.Services
{
    public class TvMazeAPI
    {
        private readonly ILogger<TvMazeAPI> _logger;
        private readonly HttpClient _httpClient;

        public TvMazeAPI(
            ILogger<TvMazeAPI> logger,
            HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<int> GetLastShowPage()
        {
            int lastPage = 0;
            var tasks = new List<Task>();

            for (var i = lastPage; i < 500; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var page = i;
                    lastPage = Math.Max(lastPage, page);
                }));
            }
            await Task.WhenAll(tasks);

            return lastPage;
        }
    }
}

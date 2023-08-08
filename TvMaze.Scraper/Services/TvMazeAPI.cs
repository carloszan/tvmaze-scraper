using Polly;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TvMaze.Scraper.Services.Dtos;

namespace TvMaze.Scraper.Services
{
    public class TvMazeApi : ITvMazeApi
    {
        public const string ClientName = "TvMazeApi";
        private readonly ILogger<TvMazeApi> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly List<ShowDto> _db;

        public TvMazeApi(
            ILogger<TvMazeApi> logger,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _db = new List<ShowDto>(100_000);
        }

        public async Task<List<ShowDto>> GetShows(int startPage = 0, int totalPages = 0)
        {
            var httpClient = _httpClientFactory.CreateClient(ClientName);
            int concurrentRequests = Environment.ProcessorCount;
            int maxRequestsPerSecond = 10;

            var semaphoreSlim = new SemaphoreSlim(maxRequestsPerSecond, maxRequestsPerSecond);

            var tasks = new Task[concurrentRequests];
            for (int pageIndex = startPage; pageIndex < totalPages; pageIndex += concurrentRequests)
            {
                for (int i = 0; i < concurrentRequests; i++)
                {
                    int currentPage = pageIndex + i;
                    if (currentPage >= totalPages)
                        break;

                    await semaphoreSlim.WaitAsync();

                    tasks[i] = GetPageAsync(httpClient, $"{currentPage}", semaphoreSlim);
                }

                tasks = tasks.Where(task => task != null).ToArray();
                await Task.WhenAll(tasks);
            }


            return _db
                .OrderBy(x => x.Id)
                .ToList() ?? throw new Exception("Can't get any shows...");
        }

        private async Task GetPageAsync(HttpClient httpClient, string page, SemaphoreSlim semaphoreSlim)
        {
            try
            {
                var showsDto = await httpClient.GetFromJsonAsync<List<ShowDto>>($"/shows?page={page}");
                //var showsDto = await httpClient.GetFromJsonAsync<List<ShowDto>>($"/429");

                if (showsDto != null)
                {
                    _db.AddRange(showsDto);
                }
            }
            catch(HttpRequestException e)
            {
                _logger.LogWarning($"Http Response: {e.StatusCode}");
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }
    }
}

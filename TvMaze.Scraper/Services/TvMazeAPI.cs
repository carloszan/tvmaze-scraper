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

        /// <summary>
        /// Return all shows from TvMazeApi
        /// </summary>
        /// <param name="startPage">First page to look up</param>
        /// <param name="totalPages">Last page to look up</param>
        /// <param name="concurrentRequests">Concurrent requests</param>
        /// <param name="maxRequestsPerSecond">Max requests per second</param>
        /// <returns></returns>
        /// <exception cref="Exception">Exception is throw if any show was found</exception>
        public async Task<List<ShowDto>> GetShows(int startPage = 0, int totalPages = 0, int concurrentRequests = 1, int maxRequestsPerSecond = 10)
        {
            var httpClient = _httpClientFactory.CreateClient(ClientName);
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

        public async Task<List<CastDto>> GetCast(int showId)
        {
            var httpClient = _httpClientFactory.CreateClient(ClientName);
            try 
            {
                var cast = await httpClient.GetFromJsonAsync<List<CastDto>>($"/shows/{showId}/cast");
                return cast ?? throw new Exception($"Couldn't get casts from show id {showId}");
            }
            catch(HttpRequestException e)
            {
                _logger.LogWarning($"Http Response: {e.StatusCode}");
            }
            return new List<CastDto>();
        }

        private async Task GetPageAsync(HttpClient httpClient, string page, SemaphoreSlim semaphoreSlim)
        {
            try
            {
                var showsDto = await httpClient.GetFromJsonAsync<List<ShowDto>>($"/shows?page={page}");

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

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
        private int _lastPage = -1;

        public TvMazeApi(
            ILogger<TvMazeApi> logger,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _db = new List<ShowDto>(100_000);
        }

        public async Task<TvMazeApiGetShowsDto> GetShows(int startPage = 0, int totalPages = 0, int concurrentRequests = 1, int maxRequestsPerSecond = 10)
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

                    tasks[i] = GetPageAsync(httpClient, currentPage, semaphoreSlim);
                }

                tasks = tasks.Where(task => task != null).ToArray();
                await Task.WhenAll(tasks);
            }

            var pages = _lastPage;
            var value = _db
                .Where(x => x != null)
                .OrderBy(x => x.Id)
                .ToList() ?? throw new Exception("Can't get any shows...");

            return new TvMazeApiGetShowsDto
            {
                Pages = pages,
                Value = value
            };
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

        private async Task GetPageAsync(HttpClient httpClient, int page, SemaphoreSlim semaphoreSlim)
        {
            try
            {
                var showsDto = await httpClient.GetFromJsonAsync<List<ShowDto>>($"/shows?page={page}");

                if (showsDto != null)
                {
                    _lastPage = Math.Max(_lastPage, page);
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

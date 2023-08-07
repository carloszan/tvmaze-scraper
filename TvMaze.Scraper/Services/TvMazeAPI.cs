using Polly;
using System.Net;

namespace TvMaze.Scraper.Services
{
    class Response
    {
        public int Page { get; set; }
        public HttpStatusCode Code { get; set; }
    }

    public class TvMazeApi : ITvMazeApi
    {
        public const string ClientName = "TvMazeApi";
        private readonly ILogger<TvMazeApi> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly List<Response> _db;

        public TvMazeApi(
            ILogger<TvMazeApi> logger,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _db = new List<Response>();
        }

        public async Task<int> GetLastShowPage()
        {
            var httpClient = _httpClientFactory.CreateClient(ClientName);
            int concurrentRequests = Environment.ProcessorCount;
            int maxRequestsPerSecond = 10;

            var semaphoreSlim = new SemaphoreSlim(maxRequestsPerSecond, maxRequestsPerSecond);

            // Get last page somewhere
            var lastPage = 0;
            int totalPages = 300;

            var tasks = new Task[concurrentRequests];
            for (int pageIndex = lastPage; pageIndex < totalPages; pageIndex += concurrentRequests)
            {
                for (int i = 0; i < concurrentRequests; i++)
                {
                    int currentPage = pageIndex + i;
                    if (currentPage >= totalPages)
                        break;

                    await semaphoreSlim.WaitAsync();

                    tasks[i] = GetPageAsync(httpClient, $"{currentPage}", semaphoreSlim);
                }

                await Task.WhenAll(tasks);
            }


            lastPage = _db
                .OrderByDescending(x => x.Page)
                .FirstOrDefault().Page;
            return lastPage;
        }

        private async Task GetPageAsync(HttpClient httpClient, string page, SemaphoreSlim semaphoreSlim)
        {
            var retryPolicy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(10));

            await retryPolicy.ExecuteAsync(async () => 
            {
                var response = await httpClient.GetAsync($"/shows?page={page}");

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    _db.Add(new Response { Page = Convert.ToInt32(page), Code = response.StatusCode });
                }

                semaphoreSlim.Release();
            });
            
        }
    }
}

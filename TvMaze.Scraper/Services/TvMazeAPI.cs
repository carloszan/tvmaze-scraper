using Polly;
using System.Net;

namespace TvMaze.Scraper.Services
{
    public class TvMazeApi : ITvMazeApi
    {
        public const string ClientName = "TvMazeApi";
        private readonly ILogger<TvMazeApi> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public TvMazeApi(
            ILogger<TvMazeApi> logger,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<int> GetLastShowPage()
        {
            var client = _httpClientFactory.CreateClient(ClientName);
            var semaphoreSlim = new SemaphoreSlim(1, 1); // Max concurrent requests = 1
            var rateLimitPolicy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(10));

            // Get last page somewhere
            int lastPage = 280;

            var tasks = new List<Task>();
            HttpStatusCode statusCode = HttpStatusCode.OK;

            int page = lastPage;
            while (statusCode == HttpStatusCode.OK)
            {
                await rateLimitPolicy.ExecuteAsync(async () =>
                {
                    await semaphoreSlim.WaitAsync();

                    try
                    {
                        var response = await client.GetAsync($"/shows?page={page}");
                        statusCode = response.StatusCode;

                        if (statusCode == HttpStatusCode.OK)
                        {
                            lastPage = page;
                        }
                    }
                    finally
                    {
                        semaphoreSlim.Release();
                    }
                });

            page += 1;
        }

            return lastPage;
        }
    }
}

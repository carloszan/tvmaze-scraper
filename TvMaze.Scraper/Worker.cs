using TvMaze.Scraper.Repositories;
using TvMaze.Scraper.Services;

namespace TvMaze.Scraper
{
    public class Worker : IHostedService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ITvMazeApi _tvMazeApi;
        private readonly IHostApplicationLifetime _hostLifetime;
        private readonly IConfiguration _configuration;
        private readonly ICache _cache;


        public Worker(
            ILogger<Worker> logger,
            ITvMazeApi tvMazeApi,
            IHostApplicationLifetime hostLifetime,
            IConfiguration configuration,
            ICache cache)
        {
            _logger = logger;
            _tvMazeApi = tvMazeApi;
            _hostLifetime = hostLifetime;
            _configuration = configuration;
            _cache = cache;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var settings = _configuration.GetSection(nameof(ScraperSettings)).Get<ScraperSettings>();

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var firstPage = await _cache.GetAsync("FIRST_PAGE");
            var lastPage = settings.LastPageToScrape;

            var showsDto = await _tvMazeApi.GetShows(Convert.ToInt32(firstPage), lastPage);

            _logger.LogInformation($"We've got {showsDto.Count} shows.");

            _hostLifetime.StopApplication();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
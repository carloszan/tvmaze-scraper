using TvMaze.Scraper.Services;

namespace TvMaze.Scraper
{
    public class Worker : IHostedService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ITvMazeApi _tvMazeApi;
        private readonly IHostApplicationLifetime _hostLifetime;
        private readonly IConfiguration _configuration;


        public Worker(ILogger<Worker> logger, ITvMazeApi tvMazeApi, IHostApplicationLifetime hostLifetime, IConfiguration configuration)
        {
            _logger = logger;
            _tvMazeApi = tvMazeApi;
            _hostLifetime = hostLifetime;
            _configuration = configuration;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var settings = _configuration.GetSection(nameof(ScraperSettings)).Get<ScraperSettings>();

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            // Get this value somewhere
            var firstPage = 0;
            var lastPage = settings.LastPageToScrape;

            var showsDto = await _tvMazeApi.GetShows(firstPage, lastPage);

            _logger.LogInformation($"We've got {showsDto.Count} shows.");

            _hostLifetime.StopApplication();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
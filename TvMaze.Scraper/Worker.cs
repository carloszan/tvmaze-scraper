using System.Threading;
using TvMaze.Scraper.Services;

namespace TvMaze.Scraper
{
    public class Worker : IHostedService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ITvMazeApi _tvMazeApi;

        public Worker(ILogger<Worker> logger, ITvMazeApi tvMazeApi)
        {
            _logger = logger;
            _tvMazeApi = tvMazeApi;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var page = await _tvMazeApi.GetLastShowPage();

            _logger.LogInformation($"Last page is {page}");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

    }
}
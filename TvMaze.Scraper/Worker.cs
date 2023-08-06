using System.Threading;
using TvMaze.Scraper.Services;

namespace TvMaze.Scraper
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ITvMazeApi _tvMazeApi;

        public Worker(ILogger<Worker> logger, ITvMazeApi tvMazeApi)
        {
            _logger = logger;
            _tvMazeApi = tvMazeApi;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            var page = await _tvMazeApi.GetLastShowPage();

            Console.WriteLine(page);
        }
    }
}
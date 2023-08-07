using Microsoft.Extensions.Hosting;
using System.Threading;
using TvMaze.Scraper.Services;

namespace TvMaze.Scraper
{
    public class Worker : IHostedService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ITvMazeApi _tvMazeApi;
        private readonly IHostApplicationLifetime _hostLifetime;


        public Worker(ILogger<Worker> logger, ITvMazeApi tvMazeApi, IHostApplicationLifetime hostLifetime)
        {
            _logger = logger;
            _tvMazeApi = tvMazeApi;
            _hostLifetime = hostLifetime;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
             var page = await _tvMazeApi.GetLastShowPage();

            _logger.LogInformation($"Last page is {page}");

            _hostLifetime.StopApplication();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

    }
}
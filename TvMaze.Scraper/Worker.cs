using TvMaze.Scraper.Repositories;
using TvMaze.Scraper.Services;

namespace TvMaze.Scraper
{
    class Cast
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required string Birthday { get; set; }
    }

    class Show
    {
        public Show(int id, string? name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; set; }
        public string? Name { get; set; }

        public List<Cast> Cast { get; set; }
    }

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

            var firstPage = Convert.ToInt32(await _cache.GetAsync("FIRST_PAGE"));
            var lastPage = settings.LastPageToScrape;

            _logger.LogInformation("Looking for shows...");
            var tvMazeApiResult = await _tvMazeApi.GetShows(firstPage, lastPage, Environment.ProcessorCount, 10);
            var showsDto = tvMazeApiResult.Value;
            _logger.LogInformation($"We've got {showsDto.Count} shows.");

            
            _logger.LogInformation("Looking for casts...");

            var shows = new List<Show>(100_000);

            var options = new ParallelOptions { MaxDegreeOfParallelism = 4, CancellationToken = cancellationToken };
            await Parallel.ForEachAsync(showsDto, options, async (showDto, cancelattionToken) =>
            {
                var castDto = await _tvMazeApi.GetCast(showDto.Id);
                var show = new Show(showDto.Id, showDto.Name);
                var cast = castDto
                    .Select(c => new Cast { Id = c.Person.Id, Name = c.Person.Name, Birthday = c.Person.Birthday })
                    .ToList();
                show.Cast = cast;

                shows.Add(show);
            });

            _logger.LogInformation("All casts were loaded...");

            _logger.LogInformation("Saving last page in the cache database...");
            var newFirstPage = tvMazeApiResult.Pages;
            await _cache.SetAsync("FIRST_PAGE", newFirstPage.ToString());
            _logger.LogInformation("Last page saved...");


            _hostLifetime.StopApplication();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
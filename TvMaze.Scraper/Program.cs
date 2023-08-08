using Polly;
using Polly.Extensions.Http;
using StackExchange.Redis;
using TvMaze.Scraper;
using TvMaze.Scraper.Repositories;
using TvMaze.Scraper.Services;

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(1, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
                                                                    retryAttempt)));
}

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddOptions();
        services.AddHostedService<Worker>();

        services.AddHttpClient("TvMazeApi", client =>
        {
            client.BaseAddress = new Uri("https://api.tvmaze.com");
            //client.BaseAddress = new Uri("https://httpstat.us");
        })
        .AddPolicyHandler(GetRetryPolicy());

        services.AddTransient<ITvMazeApi, TvMazeApi>();
        services.AddTransient<ICache, RedisCacheDb>();

        var redisSettings = config.GetSection(nameof(RedisDbSettings)).Get<RedisDbSettings>();
        if (redisSettings == null)
        {
            redisSettings = new RedisDbSettings();
        }

        var multiplexer = ConnectionMultiplexer.Connect(redisSettings.ConnectionString);
        services.AddSingleton<IConnectionMultiplexer>(multiplexer);
    })
    .Build();

host.Run();

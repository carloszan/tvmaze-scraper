using Polly;
using Polly.Extensions.Http;
using StackExchange.Redis;
using System.Net.Http.Headers;
using TvMaze.Scraper;
using TvMaze.Scraper.Repositories;
using TvMaze.Scraper.Services;

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        .WaitAndRetryForeverAsync(retryAttempt => TimeSpan.FromSeconds(10));
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
            var product = new ProductInfoHeaderValue("TvMazeScraperBot", "1.0");
            var owner = new ProductInfoHeaderValue("(+https://czar.dev)");

            client.DefaultRequestHeaders.UserAgent.Add(product);
            client.DefaultRequestHeaders.UserAgent.Add(owner);
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

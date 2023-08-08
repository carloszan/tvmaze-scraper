using Polly;
using Polly.Extensions.Http;
using TvMaze.Scraper;
using TvMaze.Scraper.Services;

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(1, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
                                                                    retryAttempt)));
}

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
    })
    .Build();

host.Run();

using Polly;
using TvMaze.Scraper;
using TvMaze.Scraper.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();

        services.AddHttpClient("TvMazeApi", client =>
        {
            client.BaseAddress = new Uri("https://api.tvmaze.com");
        });
        //.AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(5, retryAttempt => 
        //    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

        services.AddTransient<ITvMazeApi, TvMazeApi>();
    })
    .Build();

host.Run();

﻿using Polly;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TvMaze.Scraper.Services.Dtos;

namespace TvMaze.Scraper.Services
{
    public class TvMazeApi : ITvMazeApi
    {
        public const string ClientName = "TvMazeApi";
        private readonly ILogger<TvMazeApi> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly List<ShowDto> _db;

        public TvMazeApi(
            ILogger<TvMazeApi> logger,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _db = new List<ShowDto>();
        }

        public async Task<int> GetLastShowPage()
        {
            var httpClient = _httpClientFactory.CreateClient(ClientName);
            int concurrentRequests = Environment.ProcessorCount;
            int maxRequestsPerSecond = 10;

            var semaphoreSlim = new SemaphoreSlim(maxRequestsPerSecond, maxRequestsPerSecond);

            // Get last page somewhere
            var lastPage = 280;
            int totalPages = 300;

            var tasks = new Task[concurrentRequests];
            for (int pageIndex = lastPage; pageIndex < totalPages; pageIndex += concurrentRequests)
            {
                for (int i = 0; i < concurrentRequests; i++)
                {
                    int currentPage = pageIndex + i;
                    if (currentPage >= totalPages)
                        break;

                    await semaphoreSlim.WaitAsync();

                    tasks[i] = GetPageAsync(httpClient, $"{currentPage}", semaphoreSlim);
                }

                tasks = tasks.Where(task => task != null).ToArray();
                await Task.WhenAll(tasks);
            }


            var showDto = _db
                .OrderByDescending(x => x.Id)
                .FirstOrDefault();
            if (showDto == null)
            {
                return -1;
            }

            lastPage = (showDto.Id / 250) + 1;
            return lastPage;
        }

        private async Task GetPageAsync(HttpClient httpClient, string page, SemaphoreSlim semaphoreSlim)
        {
            try
            {
                //var showsDto = await httpClient.GetFromJsonAsync<List<ShowDto>>($"/shows?page={page}");
                var showsDto = await httpClient.GetFromJsonAsync<List<ShowDto>>($"/429");

                if (showsDto != null)
                {
                    _db.AddRange(showsDto);
                }

            }
            catch(HttpRequestException e)
            {
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }
    }
}

using MongoDB.Driver;
using TvMaze.Scraper.Entities;

namespace TvMaze.Scraper.Repositories
{
    public class ShowRepository : IShowRepository
    {
        private const string DatabaseName = "TvMaze";
        private const string collectionName = nameof(ShowRepository);
        private readonly ILogger<ShowRepository> _logger;
        private readonly IMongoCollection<ShowEntity> _showCollection;

        public ShowRepository(
            ILogger<ShowRepository> logger,
            IMongoClient mongoClient) 
        {
            _logger = logger;
            IMongoDatabase database = mongoClient.GetDatabase(DatabaseName);
            _showCollection = database.GetCollection<ShowEntity>(collectionName);
        }

        public async Task InsertManyAsync(List<ShowEntity> shows)
        {
            try
            {
                await _showCollection.InsertManyAsync(shows);
            }
            catch(Exception ex) 
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}

using MongoDB.Bson;
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

        public async Task UpsertManyAsync(List<ShowEntity> shows)
        {
            List<WriteModel<ShowEntity>> bulkOperations = new();

            foreach (var document in shows)
            {
                var filter = Builders<ShowEntity>.Filter.Eq("_id", document.Id);
                var update = Builders<ShowEntity>.Update
                    .Set("Name", document.Name)
                    .Set("Cast", document.Cast);
                var upsertOne = new UpdateOneModel<ShowEntity>(filter, update) { IsUpsert = true };
                bulkOperations.Add(upsertOne);
            }

            try
            {
                var bulkWriteOptions = new BulkWriteOptions { IsOrdered = true };
                await _showCollection.BulkWriteAsync(bulkOperations, bulkWriteOptions);
            }
            catch(Exception ex) 
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}

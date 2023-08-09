namespace TvMaze.Scraper.Repositories
{
    /// <summary>
    /// The ICache interface defines a set of methods that represent caching operations, 
    /// allowing for efficient data retrieval and storage. 
    /// Caching is a technique used to store frequently accessed data in a high-speed storage location,
    /// reducing the need to retrieve the data from a slower data source.
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// Get value from key
        /// </summary>
        /// <param name="key">Key to look up</param>
        /// <returns>Value from key</returns>
        Task<string> GetAsync(string key);

        /// <summary>
        /// Set key and value in the database
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <exception cref="Exception">Could not set this key value</exception>
        Task SetAsync(string key, string value);
    }
}

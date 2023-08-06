using TvMaze.Scraper.Services;

namespace TvMaze.Tests.Services
{
    [TestClass]
    public class TvMazeAPI_Tests
    {
        [TestMethod]
        public async Task TvMazeAPI_WithValidParameters_ReturnsLastShowPage()
        {
            var service = new TvMazeAPI();

            var lastPage = await service.GetLastShowPage();

            Assert.IsTrue(lastPage > 0);
        }
    }
}

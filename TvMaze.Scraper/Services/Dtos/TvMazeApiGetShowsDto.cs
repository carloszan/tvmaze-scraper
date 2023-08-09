namespace TvMaze.Scraper.Services.Dtos
{
    public class TvMazeApiGetShowsDto
    {
        public required int Pages { get; set; }
        public required List<ShowDto> Value { get; set; }
    }
}

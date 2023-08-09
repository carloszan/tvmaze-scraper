namespace TvMaze.Scraper.Services.Dtos
{
    public class PersonDto
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required string Birthday { get; set; }
    }

    public class CastDto
    {
        public required PersonDto Person { get; set; }
    }
}

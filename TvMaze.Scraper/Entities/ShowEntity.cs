namespace TvMaze.Scraper.Entities
{
    public class Cast
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required string Birthday { get; set; }
    }

    public class ShowEntity
    {
        public ShowEntity(int id, string? name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; set; }
        public string? Name { get; set; }

        public List<Cast> Cast { get; set; }
    }
}

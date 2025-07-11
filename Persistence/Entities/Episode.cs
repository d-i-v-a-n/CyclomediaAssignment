namespace Persistence.Entities;

public sealed class Episode
{
    public int Id { get; set; }
    public int TvMazeId { get; set; }
    public string? Name { get; set; }
    public int? SeasonNumber { get; set; }
    public int? EpisodeNumber { get; set; }
    public string? Summary { get; set; }
    public int SeasonId { get; set; }
    public Season Season { get; set; } = null!;
}
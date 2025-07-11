namespace Persistence.Entities;

public sealed class Show
{
    public int Id { get; set; }
    public int TvMazeId { get; set; }
    public string? Name { get; set; }
    public string? Summary { get; set; }

    public ICollection<Season> Seasons { get; set; } = [];
}

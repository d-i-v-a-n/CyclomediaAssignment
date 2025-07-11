namespace Persistence.Entities;

public sealed class Season
{
    public int Id { get; set; }
    public int TvMazeId { get; set; }
    public string? Name { get; set; }
    public int? Number { get; set; }
    public int ShowId { get; set; }
    public Show Show { get; set; } = null!;

    public ICollection<Episode> Episodes { get; set; } = [];
}

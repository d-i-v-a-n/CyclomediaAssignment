using Microsoft.EntityFrameworkCore;
using Persistence.Entities;

namespace Persistence;

internal sealed class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    public DbSet<Show> Shows { get; set; }
    public DbSet<Season> Seasons { get; set; }
    public DbSet<Episode> Episodes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Show>()
            .HasIndex(x => x.TvMazeId)
            .IsUnique();
        modelBuilder.Entity<Show>()
            .Property(_ => _.Name)
            .HasMaxLength(100);

        modelBuilder.Entity<Season>()
            .HasIndex(x => x.TvMazeId)
            .IsUnique();
        modelBuilder.Entity<Season>()
            .Property(_ => _.Name)
            .HasMaxLength(100);

        modelBuilder.Entity<Episode>()
            .HasIndex(x => x.TvMazeId)
            .IsUnique();
        modelBuilder.Entity<Episode>()
            .Property(_ => _.Name)
            .HasMaxLength(300);
    }
}

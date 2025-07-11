using Microsoft.EntityFrameworkCore;
using Persistence.Entities;

namespace Persistence;

internal sealed class ShowRepository(DatabaseContext context) : IShowRepository
{
    private readonly DatabaseContext _context = context;

    public Task<int?> GetMaxTvMazeIdAsync(CancellationToken cancellationToken) =>
        _context.Shows.MaxAsync(s => (int?)s.TvMazeId, cancellationToken);

    public async Task AddOrUpdateShowGraphAsync(Show show, CancellationToken cancellationToken)
    {
        var existingShow = await _context.Shows
            .Include(s => s.Seasons)
            .ThenInclude(se => se.Episodes)
            .FirstOrDefaultAsync(s => s.TvMazeId == show.TvMazeId, cancellationToken);

        if (existingShow == null)
        {
            _context.Shows.Add(show);
        }
        else
        {
            existingShow.Name = show.Name;
            existingShow.Summary = show.Summary;

            foreach (var season in show.Seasons)
            {
                var existingSeason = existingShow.Seasons
                    .FirstOrDefault(s => s.TvMazeId == season.TvMazeId);

                if (existingSeason == null)
                {
                    existingShow.Seasons.Add(season);
                }
                else
                {
                    existingSeason.Name = season.Name;
                    existingSeason.Number = season.Number;

                    foreach (var episode in season.Episodes)
                    {
                        var existingEpisode = existingSeason.Episodes
                            .FirstOrDefault(e => e.TvMazeId == episode.TvMazeId);

                        if (existingEpisode == null)
                        {
                            existingSeason.Episodes.Add(episode);
                        }
                        else
                        {
                            existingEpisode.Name = episode.Name;
                            existingEpisode.SeasonNumber = episode.SeasonNumber;
                            existingEpisode.EpisodeNumber = episode.EpisodeNumber;
                            existingEpisode.Summary = episode.Summary;
                        }
                    }
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Show>> Search(string partialName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(partialName))
            return Enumerable.Empty<Show>();
        return await _context.Shows
            .Where(s => EF.Functions.Like(s.Name, $"%{partialName}%"))
            .ToListAsync(cancellationToken);
    }

    public Task<List<Episode>> GetEpisodesByShowIdAsync(int showId, CancellationToken cancellationToken) =>
        _context.Episodes
            .Where(e => e.Season.Show.Id == showId)
            .ToListAsync(cancellationToken);

}

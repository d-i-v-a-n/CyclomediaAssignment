using Persistence.Entities;

namespace Scraper.Services;

internal interface ITvMazeApiClient
{
    Task<IEnumerable<Show>> GetShowsPageAsync(int page, CancellationToken cancellationToken);
    Task<IEnumerable<Season>> GetSeasonsForShowAsync(int showId, CancellationToken cancellationToken);
    Task<IEnumerable<Episode>> GetEpisodesForSeasonAsync(int seasonId, CancellationToken cancellationToken);
}

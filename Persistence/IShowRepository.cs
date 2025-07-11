using Persistence.Entities;

namespace Persistence;

public interface IShowRepository
{
    Task AddOrUpdateShowGraphAsync(Show show, CancellationToken cancellationToken);
    Task<List<Episode>> GetEpisodesByShowIdAsync(int showId, CancellationToken cancellationToken);
    Task<int?> GetMaxTvMazeIdAsync(CancellationToken cancellationToken);
    Task<IEnumerable<Show>> Search(string partialName, CancellationToken cancellationToken);
}
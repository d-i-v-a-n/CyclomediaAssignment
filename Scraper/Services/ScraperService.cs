using Microsoft.Extensions.Logging;
using Persistence;

namespace Scraper.Services;

internal sealed class ScraperService
{
    private readonly ITvMazeApiClient _apiClient;
    private readonly IShowRepository _repository;
    private readonly ILogger<ScraperService> _logger;

    public ScraperService(ITvMazeApiClient apiClient, IShowRepository repository, ILogger<ScraperService> logger)
    {
        _apiClient = apiClient;
        _repository = repository;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var maxTvMazeId = await _repository.GetMaxTvMazeIdAsync(cancellationToken);
        int page = maxTvMazeId.HasValue ? maxTvMazeId.Value / 250 : 0;

        while (true)
        {
            _logger.LogInformation("Fetching shows page {page}", page);

            var shows = await _apiClient.GetShowsPageAsync(page, cancellationToken);
            if (!shows.Any())
            {
                _logger.LogInformation("No more pages.");
                break;
            }

            foreach (var show in shows)
            {
                _logger.LogInformation("Processing show {Name} (ID: {TvMazeId}), page {page}", show.Name, show.TvMazeId, page);
                var seasons = (await _apiClient.GetSeasonsForShowAsync(show.TvMazeId, cancellationToken)).ToList();

                foreach (var season in seasons)
                {
                    var episodes = (await _apiClient.GetEpisodesForSeasonAsync(season.TvMazeId, cancellationToken)).ToList();
                    season.Episodes = episodes;
                }

                show.Seasons = seasons;

                await _repository.AddOrUpdateShowGraphAsync(show, cancellationToken);
            }

            page++;
        }
    }
}
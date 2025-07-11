using Microsoft.Extensions.Logging;
using Persistence.Entities;
using Polly;
using Polly.Retry;
using System.Net.Http.Json;
using System.Threading.RateLimiting;

namespace Scraper.Services;

internal sealed class TvMazeApiClient : ITvMazeApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly TokenBucketRateLimiter _rateLimiter;
    private readonly ILogger<TvMazeApiClient> _logger;

    public TvMazeApiClient(HttpClient httpClient, ILogger<TvMazeApiClient> logger)
    {
        _httpClient = httpClient;

        // Retry policy
        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
            );

        // Throttle: max 20 requests every 10 seconds
        _rateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = 20,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 100,
            ReplenishmentPeriod = TimeSpan.FromSeconds(10),
            TokensPerPeriod = 20,
            AutoReplenishment = true
        });
        _logger = logger;
    }

    public async Task<IEnumerable<Show>> GetShowsPageAsync(int page, CancellationToken cancellationToken)
    {
        using var lease = await _rateLimiter.AcquireAsync(1, cancellationToken);
        if (!lease.IsAcquired)
            throw new InvalidOperationException("Rate limit exceeded");

        _logger.LogInformation("Fetching shows page {page}", page);

        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _httpClient.GetFromJsonAsync<List<TvMazeShowDto>>($"shows?page={page}", cancellationToken);

                return response?.Select(dto => new Show
                {
                    TvMazeId = dto.Id,
                    Name = dto.Name,
                    Summary = dto.Summary
                }) ?? [];
            });
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogInformation("Received 404 from TVMaze for show page {page}. Stop processing.", page);
            return [];
        }
    }

    private record TvMazeShowDto(int Id, string Name, string Summary);

    public async Task<IEnumerable<Season>> GetSeasonsForShowAsync(int showId, CancellationToken cancellationToken)
    {
        using var lease = await _rateLimiter.AcquireAsync(1, cancellationToken);
        if (!lease.IsAcquired)
            throw new InvalidOperationException("Rate limit exceeded");

        _logger.LogInformation("Fetching seasons for show {showId}", showId);

        var response = await _retryPolicy.ExecuteAsync(async () =>
            await _httpClient.GetFromJsonAsync<List<TvMazeSeasonDto>>(
                $"shows/{showId}/seasons",
                cancellationToken
            )
        );

        return response?.Select(dto => new Season
        {
            TvMazeId = dto.Id,
            Name = dto.Name,
            Number = dto.Number
        }) ?? [];
    }

    private record TvMazeSeasonDto(int Id, string Name, int? Number);

    public async Task<IEnumerable<Episode>> GetEpisodesForSeasonAsync(int seasonId, CancellationToken cancellationToken)
    {
        using var lease = await _rateLimiter.AcquireAsync(1, cancellationToken);
        if (!lease.IsAcquired)
            throw new InvalidOperationException("Rate limit exceeded");

        _logger.LogInformation("Fetching episodes for season {seasonId}", seasonId);

        var response = await _retryPolicy.ExecuteAsync(async () =>
            await _httpClient.GetFromJsonAsync<List<TvMazeEpisodeDto>>(
                $"seasons/{seasonId}/episodes",
                cancellationToken));

        return response?.Select(dto => new Episode
        {
            TvMazeId = dto.Id,
            Name = dto.Name,
            SeasonNumber = dto.Season,
            EpisodeNumber = dto.Number,
            Summary = dto.Summary
        }) ?? [];
    }

    private record TvMazeEpisodeDto(int Id, string Name, int? Season, int? Number, string Summary);
}

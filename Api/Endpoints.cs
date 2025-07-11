using Microsoft.AspNetCore.Mvc;
using Persistence;

namespace Api;

public static class Endpoints
{
    public static void MapEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/shows/search", async (string name, [FromServices] IShowRepository repo, CancellationToken cancellationToken) =>
        {
            var shows = await repo.Search(name, cancellationToken);
            return shows.Select(s => new
            {
                Id = s.Id,
                Name = s.Name,
                Summary = s.Summary
            });
        })
        .WithName("SearchShows")
        .WithOpenApi();

        app.MapGet("/shows/{id:int}/episodes", async (int id, [FromServices] IShowRepository repo, CancellationToken cancellationToken) =>
        {
            var episodes = await repo.GetEpisodesByShowIdAsync(id, cancellationToken);
            return episodes.Select(e => new
            {
                SeasonNumber = e.SeasonNumber,
                EpisodeNumber = e.EpisodeNumber,
                Title = e.Name,
                Summary = e.Summary
            });
        })
        .WithName("GetEpisodesByShowId")
        .WithOpenApi();
    }
}

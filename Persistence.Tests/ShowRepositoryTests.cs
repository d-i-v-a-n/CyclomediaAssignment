using Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Tests;

public class ShowRepositoryTests
{
    private readonly DatabaseContext _context;
    private readonly ShowRepository _showRepository;

    public ShowRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB per test class instance
            .Options;

        _context = new DatabaseContext(options);

        // Seed test data
        var show1 = new Show
        {
            Name = "Test Show",
            TvMazeId = 1,
            Summary = "A test show",
            Seasons =
            [
                new Season
                {
                    Name = "Season 1",
                    Number = 1,
                    TvMazeId = 101,
                    Episodes =
                    [
                        new() {
                            Name = "Episode 1",
                            TvMazeId = 1001,
                            SeasonNumber = 1,
                            EpisodeNumber = 1,
                            Summary = "First episode"
                        },
                        new Episode
                        {
                            Name = "Episode 2",
                            TvMazeId = 1002,
                            SeasonNumber = 1,
                            EpisodeNumber = 2,
                            Summary = "Second episode"
                        }
                    ]
                }
            ]
        };

        var show2 = new Show
        {
            Name = "Another Show",
            TvMazeId = 2,
            Summary = "Another test show",
            Seasons =
            [
                new Season
                {
                    Name = "Season 1",
                    Number = 1,
                    TvMazeId = 201,
                    Episodes =
                    [
                        new() {
                            Name = "Episode 1",
                            TvMazeId = 2001,
                            SeasonNumber = 1,
                            EpisodeNumber = 1,
                            Summary = "First episode"
                        },
                        new Episode
                        {
                            Name = "Episode 2",
                            TvMazeId = 2002,
                            SeasonNumber = 1,
                            EpisodeNumber = 2,
                            Summary = "Second episode"
                        }
                    ]
                }
            ]
        };

        var show3 = new Show
        {
            Name = "-no other matches-",
            TvMazeId = 3,
            Summary = "Another test show"
        };

        _context.Shows.Add(show1);
        _context.Shows.Add(show2);
        _context.Shows.Add(show3);
        _context.SaveChanges();

        _showRepository = new ShowRepository(_context);
    }

    [Fact]
    public async Task Search_ReturnsMatchingShows()
    {
        var result = await _showRepository.Search("Test", CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("Test Show", result.First().Name);
    }

    [Fact]
    public async Task GetEpisodesByShowIdAsync_ReturnsEpisodes()
    {
        var testShow = _context.Shows.First(s => s.Name == "Test Show");
        var episodes = await _showRepository.GetEpisodesByShowIdAsync(testShow.Id, CancellationToken.None);

        Assert.Equal(2, episodes.Count);
        Assert.Contains(episodes, e => e.Name == "Episode 1" && e.TvMazeId == 1001);
        Assert.Contains(episodes, e => e.Name == "Episode 2" && e.TvMazeId == 1002);
    }

    [Theory]
    [InlineData("Show", 2)]
    [InlineData("XXXXXXXXXXXXX", 0)]
    [InlineData("-no other matches-", 1)]
    public async Task Search_ReturnsMatchingShows_Like(string showName, int expectedCount)
    {
        var result = await _showRepository.Search(showName, CancellationToken.None);

        Assert.Equal(expectedCount, result.Count());

        // all results should contain the search term
        foreach (var show in result)
        {
            Assert.Contains(showName, show.Name!, StringComparison.OrdinalIgnoreCase);
        }
    }
}
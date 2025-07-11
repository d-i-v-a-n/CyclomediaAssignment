using Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Tests;

public class ShowRepositoryTests
{
    [Fact]
    public async Task Search_ReturnsMatchingShows()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        using var context = new DatabaseContext(options);
        context.Shows.Add(new Show { Name = "Test Show" });
        context.Shows.Add(new Show { Name = "Another Show" });
        await context.SaveChangesAsync();

        var repo = new ShowRepository(context);

        var result = await repo.Search("Test", CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("Test Show", result.First().Name);
    }
}
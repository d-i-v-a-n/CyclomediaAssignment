using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Persistence;

public static class Configuration
{
    public static void AddPersistence(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(connectionString));
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Persistence;
using Scraper.Services;

var builder = Host.CreateApplicationBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not set.");

builder.Services.AddPersistence(connectionString);

builder.Services.AddLogging();

builder.Services.AddHttpClient<ITvMazeApiClient, TvMazeApiClient>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration.GetSection("TvMazeApi")["BaseAddress"]
    ?? throw new InvalidOperationException("TvMazeApi:BaseAddress not found in configuration."));
});
builder.Services.AddScoped<ScraperService>();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var scraper = scope.ServiceProvider.GetRequiredService<ScraperService>();
await scraper.RunAsync(CancellationToken.None);
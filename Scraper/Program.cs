using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Persistence;

var builder = Host.CreateApplicationBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not set.");

builder.Services.AddPersistence(connectionString);

builder.Services.AddLogging();

var app = builder.Build();
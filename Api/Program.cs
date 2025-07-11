using Api;
using Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPersistence(
    builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new Exception("Connection string not set"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapEndpoints();
app.Run();
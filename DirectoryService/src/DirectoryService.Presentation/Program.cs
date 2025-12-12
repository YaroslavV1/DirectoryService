using System.Text.Json.Serialization;
using DirectoryService.Application.Database;
using DirectoryService.Infrastructure;
using DirectoryService.Infrastructure.Database;
using DirectoryService.Presentation;
using DirectoryService.Presentation.Extensions;
using Microsoft.AspNetCore.Http.Json;
using Serilog;
using SharedService.Core.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<DirectoryServiceDbContext>(_ =>
    new DirectoryServiceDbContext(
        builder.Configuration.GetConnectionString("DirectoryServiceDb")!));

builder.Services.AddScoped<IReadDbContext, DirectoryServiceDbContext>(_ =>
    new DirectoryServiceDbContext(
        builder.Configuration.GetConnectionString("DirectoryServiceDb")!));

builder.Services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>(_ =>
    new NpgsqlConnectionFactory(builder.Configuration.GetConnectionString("DirectoryServiceDb")!));

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

builder.Services.AddProgramDependencies(builder.Configuration);

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

app.UseExceptionHandlerMiddleware();

if (app.Environment.IsDevelopment())
{
    if (args.Contains("--seeding"))
    {
        await app.Services.RunSeedingAsync();
    }
}

app.MapOpenApi();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "DirectoryService"));

await app.ApplyMigrations();

app.UseSerilogRequestLogging();

app.MapControllers();

app.Run();

namespace DirectoryService.Presentation
{
    public partial class Program;
}
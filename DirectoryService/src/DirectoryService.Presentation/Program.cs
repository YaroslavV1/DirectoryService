using System.Text.Json.Serialization;
using DirectoryService.Application.Database;
using DirectoryService.Infrastructure;
using DirectoryService.Infrastructure.Database;
using DirectoryService.Infrastructure.Seeding;
using DirectoryService.Presentation;
using DirectoryService.Presentation.Extensions;
using Microsoft.AspNetCore.Http.Json;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Debug()
    .WriteTo.Console()
    .WriteTo.Seq(builder.Configuration.GetConnectionString("Seq")
                 ?? throw new ArgumentNullException("Seq"))
    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
    .CreateLogger();

builder.Services.AddScoped<DirectoryServiceDbContext>(sp =>
    new DirectoryServiceDbContext(
        builder.Configuration.GetConnectionString("DirectoryServiceDb")!));

builder.Services.AddScoped<IReadDbContext, DirectoryServiceDbContext>(sp =>
    new DirectoryServiceDbContext(
        builder.Configuration.GetConnectionString("DirectoryServiceDb")!));

builder.Services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>(sp => 
    new NpgsqlConnectionFactory(builder.Configuration.GetConnectionString("DirectoryServiceDb")!));

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

builder.Services.AddProgramDependencies();

builder.Services.AddSerilog();

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

app.UseExceptionHandlerMiddleware();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "DirectoryService"));

    if (args.Contains("--seeding"))
    {
        await app.Services.RunSeedingAsync();
    }
}

app.UseSerilogRequestLogging();

app.MapControllers();

app.Run();

namespace DirectoryService.Presentation
{
    public partial class Program;
}
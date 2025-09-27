using DirectoryService.Infrastructure;
using DirectoryService.Presentation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<DirectoryServiceDbContext>(sp =>
    new DirectoryServiceDbContext(
        builder.Configuration.GetConnectionString("DirectoryServiceDb")!));

builder.Services.AddProgramDependencies();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "DirectoryService"));
}

app.MapControllers();

app.Run();
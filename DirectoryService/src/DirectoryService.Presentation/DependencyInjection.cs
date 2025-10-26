using DirectoryService.Application;
using DirectoryService.Infrastructure;
using DirectoryService.Shared;
using Microsoft.OpenApi.Models;

namespace DirectoryService.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddProgramDependencies(this IServiceCollection services) =>
        services.AddWebDependencies()
            .AddApplication()
            .AddInfrastructure();

    private static IServiceCollection AddWebDependencies(this IServiceCollection services)
    {
        services.AddControllers();

        services.AddOpenApi(options =>
        {
            options.AddSchemaTransformer((schema, context, _) =>
            {
                if (context.JsonTypeInfo.Type == typeof(Envelope<Errors>))
                {
                    if (schema.Properties.TryGetValue("errors", out var errorsProp))
                    {
                        errorsProp.Items.Reference = new OpenApiReference
                        {
                            Type = ReferenceType.Schema,
                        };
                    }
                }

                return Task.CompletedTask;
            });
        });

        return services;
    }
}
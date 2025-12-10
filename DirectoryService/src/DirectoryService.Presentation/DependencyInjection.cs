using DirectoryService.Application;
using DirectoryService.Application.Caching;
using DirectoryService.Infrastructure;
using DirectoryService.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace DirectoryService.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddProgramDependencies(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services.AddWebDependencies()
            .AddApplication()
            .AddDistributedCache(configuration)
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
                        errorsProp.Items.Reference = new OpenApiReference { Type = ReferenceType.Schema, };
                    }
                }

                return Task.CompletedTask;
            });
        });

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        return services;
    }

    private static IServiceCollection AddDistributedCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            string connection = configuration.GetConnectionString("Redis")
                                ?? throw new ArgumentNullException(nameof(configuration));

            options.Configuration = connection;
        });

        services.AddSingleton<ICacheService, DistributedCacheService>();

        return services;
    }
}
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Infrastructure.Seeding;

public static class SeederExtension
{
    public static async Task<IServiceProvider> RunSeedingAsync(this IServiceProvider serviceProvider)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var seeders = scope.ServiceProvider.GetServices<ISeeder>();

        foreach (var seeder in seeders)
        {
            await seeder.SeedAsync();
        }

        return serviceProvider;
    }
}
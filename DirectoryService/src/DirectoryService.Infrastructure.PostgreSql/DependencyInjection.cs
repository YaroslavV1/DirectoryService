using DirectoryService.Application.Departments;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Positions;
using DirectoryService.Infrastructure.BackgroundServices;
using DirectoryService.Infrastructure.Database;
using DirectoryService.Infrastructure.Departments;
using DirectoryService.Infrastructure.Locations;
using DirectoryService.Infrastructure.Positions;
using DirectoryService.Infrastructure.Seeding;
using Microsoft.Extensions.DependencyInjection;
using SharedService.Core.Database;

namespace DirectoryService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ILocationsRepository, LocationsRepository>();
        services.AddScoped<IDepartmentsRepository, DepartmentsRepository>();
        services.AddScoped<IPositionRepository, PositionsRepository>();
        services.AddScoped<ITransactionManager, TransactionManager>();
        services.AddScoped<ISeeder, DirectoryServiceSeeding>();

        services.AddHostedService<DepartmentCleanupBackgroundService>();
        return services;
    }
}
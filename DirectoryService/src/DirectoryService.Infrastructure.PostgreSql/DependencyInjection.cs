using DirectoryService.Application.Departments;
using DirectoryService.Application.Locations;
using DirectoryService.Infrastructure.Departments;
using DirectoryService.Infrastructure.Locations;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ILocationsRepository, LocationsRepository>();
        services.AddScoped<IDepartmentsRepository, DepartmentsRepository>();

        return services;
    }
}
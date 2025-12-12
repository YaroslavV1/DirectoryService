using DirectoryService.Application.Departments.DeleteInactiveDepartment;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SharedService.Core;

namespace DirectoryService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        var assembly = typeof(DependencyInjection).Assembly;

        services.AddHandlers(assembly);

        services.AddScoped<DeleteInactiveDepartmentHandler>();

        return services;
    }
}
using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments;

public class DepartmentBaseTests : DirectoryBaseTests
{
    public DepartmentBaseTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    public async Task<Result<Guid, Errors>> CreateDepartment(
        string name,
        string identifier,
        IEnumerable<Guid> locationIds,
        Guid? parentId = null)
    {
        var result = await ExecuteHandler(async (CreateDepartmentHandler sut) =>
        {
            var command = new CreateDepartmentCommand(new CreateDepartmentRequest(
                name,
                identifier,
                locationIds,
                parentId));

            return await sut.Handle(command, CancellationToken.None);
        });

        return result;
    }

    public async Task<LocationId> CreateLocation(CreateLocationRequest locationRequest)
    {
        return await ExecuteInDb(async (context) =>
        {
            var location = Location.Create(
                LocationId.NewId(),
                LocationName.Create(locationRequest.Name).Value,
                Address.Create(
                    locationRequest.Address.City,
                    locationRequest.Address.Street,
                    locationRequest.Address.PostalCode,
                    locationRequest.Address.House).Value,
                LocationTimeZone.Create(locationRequest.TimeZone).Value);

            context.Locations.Add(location);

            await context.SaveChangesAsync();

            return location.Id;
        });
    }

    protected async Task<TResponse> ExecuteHandler<THandler, TResponse>(
        Func<THandler, Task<TResponse>> action)
        where THandler : notnull
    {
        await using var scope = Services.CreateAsyncScope();

        var handler = scope.ServiceProvider.GetRequiredService<THandler>();

        return await action(handler);
    }
}
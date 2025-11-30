using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Contracts.Departments.CreateDepartment;
using DirectoryService.Contracts.Locations.CreateLocation;
using DirectoryService.Contracts.Positions;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.DepartmentPositions.ValueObjects;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Positions.ValueObjects;
using DirectoryService.Infrastructure;
using DirectoryService.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests;

public class DirectoryBaseTests : IClassFixture<DirectoryTestWebFactory>, IAsyncLifetime
{
    private readonly Func<Task> _resetDatabase;

    protected IServiceProvider Services { get; set; }

    public DirectoryBaseTests(DirectoryTestWebFactory factory)
    {
        Services = factory.Services;
        _resetDatabase = factory.ResetDatabaseAsync;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _resetDatabase();
    }

    protected async Task<T> ExecuteInDb<T>(Func<DirectoryServiceDbContext, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();

        return await action(dbContext);
    }

    protected async Task ExecuteInDb(Func<DirectoryServiceDbContext, Task> action)
    {
        await using var scope = Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();

        await action(dbContext);
    }

    protected async Task<TResponse> ExecuteHandler<THandler, TResponse>(
        Func<THandler, Task<TResponse>> action)
        where THandler : notnull
    {
        await using var scope = Services.CreateAsyncScope();

        var handler = scope.ServiceProvider.GetRequiredService<THandler>();

        return await action(handler);
    }


    protected async Task<LocationId> CreateLocation(CreateLocationRequest locationRequest)
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

    protected async Task<PositionId> CreatePosition(CreatePositionRequest positionRequest)
    {
        return await ExecuteInDb(async (context) =>
        {
            var positionId = PositionId.NewId();
            var departmentPositions = positionRequest.DepartmentsIds.Select(dp =>
                DepartmentPosition.Create(
                    DepartmentPositionId.NewGuid(),
                    DepartmentId.Create(dp),
                    positionId).Value);

            var position = Position.Create(
                positionId,
                PositionName.Create(positionRequest.Name).Value,
                positionRequest.Description,
                departmentPositions).Value;

            context.Positions.Add(position);

            await context.SaveChangesAsync();

            return position.Id;
        });
    }

    protected async Task<Result<Guid, Errors>> CreateDepartment(
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
}
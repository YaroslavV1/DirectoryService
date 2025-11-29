using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations.GetLocations;
using DirectoryService.Contracts.Locations.GetLocations;
using DirectoryService.Shared;

namespace DirectoryService.IntegrationTests.Locations;

public class LocationsBaseTests : DirectoryBaseTests
{
    protected LocationsBaseTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    protected async Task<Result<GetLocationsDto, Errors>> GetLocations(
        IEnumerable<Guid>? departmentIds,
        string? search,
        bool? isActive,
        int? page,
        int? pageSize,
        string? orderBy,
        string? orderByDirection)
    {
        var result = await ExecuteHandler(async (GetLocationsHandler sut) =>
        {
            var query = new GetLocationsQuery(new GetLocationsRequest(
                departmentIds,
                search,
                isActive,
                page,
                pageSize,
                orderBy,
                orderByDirection));

            return await sut.Handle(query, CancellationToken.None);
        });

        return result;
    }
}
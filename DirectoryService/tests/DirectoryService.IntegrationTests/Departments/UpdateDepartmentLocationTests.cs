using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.UpdateDepartmentLocations;
using DirectoryService.Contracts.Departments.UpdateDepartmentLocations;
using DirectoryService.Contracts.Locations.CreateLocation;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Shared;

namespace DirectoryService.IntegrationTests.Departments;

public class UpdateDepartmentLocationTests : DepartmentBaseTests
{
    public UpdateDepartmentLocationTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task UpdateDepartmentLocation_with_valid_data_should_succeed()
    {
        // Arrange
        var locationId = await CreateLocation(
            new CreateLocationRequest(
                "TestLoc",
                new CreateLocationAddressDto("City", "Street", "1", "60001"),
                "Europe/Test"));

        var locationForUpdate1 = await CreateLocation(
            new CreateLocationRequest(
                "TestLocUpdate1",
                new CreateLocationAddressDto("CityU", "StreetU", "2", "60002"),
                "Europe/TestU"));

        var department = await CreateDepartment(
            "TestDepartmentName",
            "test-identifier",
            [locationId.Value]);

        // Act
        var cancellationToken = CancellationToken.None;

        var result = await UpdateDepartmentLocations(
            department.Value,
            [locationForUpdate1.Value],
            cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateDepartmentLocation_with_invalid_data_should_failed()
    {
        // Act
        var result = await UpdateDepartmentLocations(
            DepartmentId.NewId().Value,
            []);

        // Assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task UpdateDepartmentLocation_with_nonexistent_department_should_failed()
    {
        // Arrange
        var locationForUpdate1 = await CreateLocation(
            new CreateLocationRequest(
                "TestLocUpdate1",
                new CreateLocationAddressDto("CityU", "StreetU", "2", "60002"),
                "Europe/TestU"));

        // Act
        var result = await UpdateDepartmentLocations(
            Guid.NewGuid(),
            [locationForUpdate1.Value]);

        // Assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task UpdateDepartmentLocation_with_nonexistent_location_should_failed()
    {
        // Arrange
        var locationForUpdate1 = await CreateLocation(
            new CreateLocationRequest(
                "TestLocUpdate1",
                new CreateLocationAddressDto("CityU", "StreetU", "2", "60002"),
                "Europe/TestU"));
        var locationForUpdate2 = Guid.NewGuid();

        // Act
        var result = await UpdateDepartmentLocations(
            Guid.NewGuid(),
            [
                locationForUpdate1.Value,
                locationForUpdate2
            ]);

        // Assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
    }

    private async Task<Result<Guid, Errors>> UpdateDepartmentLocations(
        Guid departmentId,
        IEnumerable<Guid> locationsIds,
        CancellationToken cancellationToken = default)
    {
        var result = await ExecuteHandler(async (UpdateDepartmentLocationsHandler sut) =>
        {
            var command = new UpdateDepartmentLocationsCommand(
                departmentId,
                new UpdateDepartmentLocationsRequest(
                    locationsIds));

            return await sut.Handle(command, cancellationToken);
        });
        return result;
    }
}
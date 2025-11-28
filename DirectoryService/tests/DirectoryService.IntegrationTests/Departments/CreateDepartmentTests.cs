using DirectoryService.Contracts.Locations;
using DirectoryService.Contracts.Locations.CreateLocation;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Departments;

public class CreateDepartmentTests : DepartmentBaseTests
{
    public CreateDepartmentTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateDepartment_with_valid_data_should_succeed()
    {
        // Arrange
        LocationId locationId = await CreateLocation(
            new CreateLocationRequest(
                "TestLoc1",
                new CreateLocationAddressDto("TestCity1", "av. Street1", "2", "60001"),
                "Europe/Test"));

        var cancellationToken = CancellationToken.None;

        var result = await CreateDepartment(
            "TestDepartmentName",
            "test-identifier",
            [locationId.Value]);

        // Assert
        var department = await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .FirstAsync(
                    d => d.Id == DepartmentId.Create(result.Value),
                    cancellationToken: cancellationToken);

            return department;
        });

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        Assert.NotNull(department);
        Assert.Equal(department.Id.Value, result.Value);
    }

    [Fact]
    public async Task CreateDepartment_with_invalid_data_should_failed()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        var result = await CreateDepartment(
            "TestDepartmentName",
            "test-identifier",
            []);

        // Assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task CreateDepartment_with_valid_parent_should_succeed()
    {
        // Arrange
        LocationId locationId = await CreateLocation(
            new CreateLocationRequest(
                "TestLoc1",
                new CreateLocationAddressDto("TestCity1", "av. Street1", "2", "60001"),
                "Europe/Test"));

        var parent = await CreateDepartment(
            "TestParentDepartmentName",
            "test-parent-identifier",
            [locationId.Value]);

        // Act
        var result = await CreateDepartment(
            "TestDepartmentName",
            "test-identifier",
            [locationId.Value],
            parent.Value);

        // Assert
        Assert.True(result.IsSuccess);

        var department = await ExecuteInDb(async dbContext =>
            await dbContext.Departments
                .FirstAsync(d => d.Id == DepartmentId.Create(result.Value)));

        Assert.NotNull(department.ParentId);
        Assert.Equal(parent.Value, department.ParentId.Value);
    }

    [Fact]
    public async Task CreateDepartment_with_nonexistent_parent_should_fail()
    {
        // Arrange
        var locationId = await CreateLocation(
            new CreateLocationRequest(
                "TestLoc",
                new CreateLocationAddressDto("City", "Street", "1", "60001"),
                "Europe/Test"));

        var nonExistentParentId = Guid.NewGuid();

        // Act
        var result = await CreateDepartment(
            "TestDepartmentName",
            "test-identifier",
            [locationId.Value],
            nonExistentParentId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task CreateDepartment_with_nonexistent_location_should_fail()
    {
        // Arrange
        var nonExistentLocationId = Guid.NewGuid();
        var locationId = await CreateLocation(
            new CreateLocationRequest(
                "TestLoc",
                new CreateLocationAddressDto("City", "Street", "1", "60001"),
                "Europe/Test"));

        // Act
        var result = await CreateDepartment(
            "TestDepartmentName",
            "test-identifier",
            [
                locationId.Value,
                nonExistentLocationId
            ]);

        // Assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task CreateDepartment_with_duplicate_locations_should_fail()
    {
        // Arrange
        var locationId = await CreateLocation(
            new CreateLocationRequest(
                "TestLoc",
                new CreateLocationAddressDto("City", "Street", "1", "60001"),
                "Europe/Test"));

        // Act
        var result = await CreateDepartment(
            "TestDepartmentName",
            "test-identifier",
            [
                locationId.Value,
                locationId.Value
            ]);

        // Assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task CreateDepartment_with_multiple_locations_should_create_all_department_locations()
    {
        // Arrange
        var locationId1 = await CreateLocation(
            new CreateLocationRequest("Loc1", new CreateLocationAddressDto("City1", "Street1", "1", "60001"),
                "Europe/Test"));
        var locationId2 = await CreateLocation(
            new CreateLocationRequest("Loc2", new CreateLocationAddressDto("City2", "Street2", "2", "60002"),
                "Europe/Test"));

        // Act
        var result = await CreateDepartment(
            "MultiLocationDept",
            "multi-loc-identifier",
            [
                locationId1.Value,
                locationId2.Value
            ]);

        // Assert
        Assert.True(result.IsSuccess);

        var department = await ExecuteInDb(async dbContext =>
            await dbContext.Departments
                .Include(d => d.Locations)
                .Where(d => d.Id == DepartmentId.Create(result.Value))
                .FirstAsync(CancellationToken.None));

        Assert.Equal(2, department.Locations.Count());
    }
}
using DirectoryService.Contracts.Locations.CreateLocation;

namespace DirectoryService.IntegrationTests.Locations;

public class GetLocationsTests : LocationsBaseTests
{
    public GetLocationsTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetLocations_with_no_filters_should_return_all_locations()
    {
        // Arrange
        await CreateLocation(new CreateLocationRequest(
            "Location1",
            new CreateLocationAddressDto("City1", "Street1", "1", "60001"),
            "Europe/Test"));

        await CreateLocation(new CreateLocationRequest(
            "Location2",
            new CreateLocationAddressDto("City2", "Street2", "2", "60002"),
            "Europe/Test"));

        // Act
        var result = await GetLocations(null, null, null, 1, 10, null, null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Locations.Count);
        Assert.Equal(2, result.Value.TotalCount);
    }

    [Fact]
    public async Task GetLocations_with_search_filter_should_return_matching_locations()
    {
        // Arrange
        await CreateLocation(new CreateLocationRequest(
            "TestLocation",
            new CreateLocationAddressDto("City1", "Street1", "1", "60001"),
            "Europe/Test"));

        await CreateLocation(new CreateLocationRequest(
            "AnotherLocation",
            new CreateLocationAddressDto("City2", "Street2", "2", "60002"),
            "Europe/Test"));

        // Act
        var result = await GetLocations(
            null,
            "Test",
            null,
            1,
            10,
            null,
            null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Locations);
        Assert.Equal("TestLocation", result.Value.Locations.First().Name);
    }

    [Fact]
    public async Task GetLocations_with_pagination_should_return_correct_page()
    {
        // Arrange
        for (int i = 1; i <= 5; i++)
        {
            await CreateLocation(new CreateLocationRequest(
                $"Location{i}",
                new CreateLocationAddressDto($"City{i}", "Street", $"{i}", "60001"),
                "Europe/Test"));
        }

        // Act
        var result = await GetLocations(
            null,
            null,
            null,
            2,
            2,
            null,
            null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Locations.Count);
        Assert.Equal(5, result.Value.TotalCount);
    }

    [Fact]
    public async Task GetLocations_with_sort_by_name_ascending_should_return_sorted()
    {
        // Arrange
        await CreateLocation(new CreateLocationRequest(
            "Zebra",
            new CreateLocationAddressDto("City1", "Street1", "1", "60001"),
            "Europe/Test"));

        await CreateLocation(new CreateLocationRequest(
            "Apple",
            new CreateLocationAddressDto("City2", "Street2", "2", "60002"),
            "Europe/Test"));

        await CreateLocation(new CreateLocationRequest(
            "Banana",
            new CreateLocationAddressDto("City3", "Street3", "3", "60003"),
            "Europe/Test"));

        // Act
        var result = await GetLocations(null, null, null, 1, 10, "name", "asc");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Locations.Count);
        Assert.Equal("Apple", result.Value.Locations.ElementAt(0).Name);
        Assert.Equal("Banana", result.Value.Locations.ElementAt(1).Name);
        Assert.Equal("Zebra", result.Value.Locations.ElementAt(2).Name);
    }

    [Fact]
    public async Task GetLocations_with_department_filter_should_return_locations_for_department()
    {
        // Arrange
        var locationId1 = await CreateLocation(new CreateLocationRequest(
            "Location1",
            new CreateLocationAddressDto("City1", "Street1", "1", "60001"),
            "Europe/Test"));

        var locationId2 = await CreateLocation(new CreateLocationRequest(
            "Location2",
            new CreateLocationAddressDto("City2", "Street2", "2", "60002"),
            "Europe/Test"));

        var departmentId = await CreateDepartment(
            "TestDepartment",
            "test-dept",
            [locationId1.Value]);

        // Act
        var result = await GetLocations([departmentId.Value], null, null, 1, 10, null, null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Locations);
        Assert.Equal("Location1", result.Value.Locations.First().Name);
    }

    [Fact]
    public async Task GetLocations_with_multiple_department_filters_should_return_locations_for_all_departments()
    {
        // Arrange
        var locationId1 = await CreateLocation(new CreateLocationRequest(
            "Location1",
            new CreateLocationAddressDto("City1", "Street1", "1", "60001"),
            "Europe/Test"));

        var locationId2 = await CreateLocation(new CreateLocationRequest(
            "Location2",
            new CreateLocationAddressDto("City2", "Street2", "2", "60002"),
            "Europe/Test"));

        var locationId3 = await CreateLocation(new CreateLocationRequest(
            "Location3",
            new CreateLocationAddressDto("City3", "Street3", "3", "60003"),
            "Europe/Test"));

        var departmentId1 = await CreateDepartment(
            "Department1",
            "dept-one",
            [locationId1.Value]);

        var departmentId2 = await CreateDepartment(
            "Department2",
            "dept-two",
            [locationId2.Value]);

        // Act
        var result = await GetLocations(
            [departmentId1.Value, departmentId2.Value],
            null,
            null,
            1,
            10,
            null,
            null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Locations.Count);
    }
}
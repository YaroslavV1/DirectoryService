using DirectoryService.Contracts.Locations.CreateLocation;
using DirectoryService.Contracts.Positions;

namespace DirectoryService.IntegrationTests.Departments;

public class GetTopDepartmentsTests : DepartmentBaseTests
{
    public GetTopDepartmentsTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetTopDepartments_with_no_limit_should_return_default_top_5()
    {
        // Arrange
        var locationId = await CreateLocation(new CreateLocationRequest(
            "LocationOne",
            new CreateLocationAddressDto("CityOne", "StreetOne", "One", "SixZeroZeroZeroOne"),
            "Europe/Test"));

        var departmentIds = new List<Guid>();

        string[] deptNames =
        [
            "TestDepOne", "TestDepTwo", "TestDepThree", "TestDepFour", "TestDepFive", "TestDepSix", "TestDepSeven",
            "TestDepEight"
        ];

        string[] deptCodes =
        [
            "test-dep-one", "test-dep-two", "test-dep-three", "test-dep-four", "test-dep-five", "test-dep-six",
            "test-dep-seven", "test-dep-eight"
        ];

        for (int i = 0; i < 8; i++)
        {
            var department = await CreateDepartment(
                deptNames[i],
                deptCodes[i],
                [locationId.Value]);

            Assert.True(department.IsSuccess);
            departmentIds.Add(department.Value);
        }

        string[] posNumbers = ["One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight"];

        for (int deptIndex = 0; deptIndex < departmentIds.Count; deptIndex++)
        {
            int positionCount = deptIndex + 1;

            for (int posIndex = 0; posIndex < positionCount; posIndex++)
            {
                await CreatePosition(new CreatePositionRequest(
                    $"PositionTest{posNumbers[deptIndex]}{posNumbers[posIndex]}",
                    null,
                    [departmentIds[deptIndex]]));
            }
        }

        // Act
        var result = await GetTopDepartments(null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.TopDepartmentsByPositionsCount.Count);
        Assert.Equal(5, result.Value.TotalDepartments);
    }

    [Fact]
    public async Task GetTopDepartments_with_custom_limit_should_return_correct_count()
    {
        // Arrange
        var locationId = await CreateLocation(new CreateLocationRequest(
            "LocationOne",
            new CreateLocationAddressDto("CityOne", "StreetOne", "One", "SixZeroZeroZeroOne"),
            "Europe/Test"));

        var departmentIds = new List<Guid>();

        string[] deptNames =
        [
            "TestDepOne", "TestDepTwo", "TestDepThree", "TestDepFour", "TestDepFive", "TestDepSix", "TestDepSeven",
            "TestDepEight"
        ];

        string[] deptCodes =
        [
            "test-dep-one", "test-dep-two", "test-dep-three", "test-dep-four", "test-dep-five", "test-dep-six",
            "test-dep-seven", "test-dep-eight"
        ];

        for (int i = 0; i < 8; i++)
        {
            var department = await CreateDepartment(deptNames[i], deptCodes[i], [locationId.Value]);
            Assert.True(department.IsSuccess);
            departmentIds.Add(department.Value);
        }

        string[] posNumbers = ["One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight"];

        for (int deptIndex = 0; deptIndex < departmentIds.Count; deptIndex++)
        {
            int positionCount = deptIndex + 1;
            for (int posIndex = 0; posIndex < positionCount; posIndex++)
            {
                await CreatePosition(new CreatePositionRequest(
                    $"PositionTest{posNumbers[deptIndex]}{posNumbers[posIndex]}",
                    null,
                    [departmentIds[deptIndex]]));
            }
        }

        // Act
        var result = await GetTopDepartments(3);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.TopDepartmentsByPositionsCount.Count);
        Assert.Equal(3, result.Value.TotalDepartments);
    }

    [Fact]
    public async Task GetTopDepartments_with_no_departments_should_return_empty_list()
    {
        // Arrange

        // Act
        var result = await GetTopDepartments(null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.TopDepartmentsByPositionsCount);
        Assert.Equal(0, result.Value.TotalDepartments);
    }

    [Fact]
    public async Task
        GetTopDepartments_with_departments_without_positions_should_return_departments_with_zero_positions()
    {
        // Arrange
        var locationId = await CreateLocation(new CreateLocationRequest(
            "LocationOne",
            new CreateLocationAddressDto("CityOne", "StreetOne", "One", "SixZeroZeroZeroOne"),
            "Europe/Test"));

        await CreateDepartment("TestDepOne", "test-dep-one", [locationId.Value]);
        await CreateDepartment("TestDepTwo", "test-dep-two", [locationId.Value]);
        await CreateDepartment("TestDepThree", "test-dep-three", [locationId.Value]);

        // Act
        var result = await GetTopDepartments(null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.TopDepartmentsByPositionsCount.Count);
        Assert.Equal(3, result.Value.TotalDepartments);
        Assert.All(result.Value.TopDepartmentsByPositionsCount,
            dept => Assert.Equal(0, dept.TotalPositions));
    }

    [Fact]
    public async Task GetTopDepartments_with_zero_or_negative_limit_should_return_default_top_5()
    {
        // Arrange
        var locationId = await CreateLocation(new CreateLocationRequest(
            "LocationOne",
            new CreateLocationAddressDto("CityOne", "StreetOne", "One", "SixZeroZeroZeroOne"),
            "Europe/Test"));

        var departmentIds = new List<Guid>();

        string[] deptNames =
        [
            "TestDepOne", "TestDepTwo", "TestDepThree", "TestDepFour", "TestDepFive", "TestDepSix", "TestDepSeven"
        ];

        string[] deptCodes =
        [
            "test-dep-one", "test-dep-two", "test-dep-three", "test-dep-four", "test-dep-five", "test-dep-six",
            "test-dep-seven"
        ];

        for (int i = 0; i < 7; i++)
        {
            var department = await CreateDepartment(deptNames[i], deptCodes[i], [locationId.Value]);
            departmentIds.Add(department.Value);
        }

        string[] posNumbers = ["One", "Two", "Three", "Four", "Five", "Six", "Seven"];

        for (int deptIndex = 0; deptIndex < departmentIds.Count; deptIndex++)
        {
            int positionCount = deptIndex + 1;
            for (int posIndex = 0; posIndex < positionCount; posIndex++)
            {
                await CreatePosition(new CreatePositionRequest(
                    $"PositionTest{posNumbers[deptIndex]}{posNumbers[posIndex]}",
                    null,
                    [departmentIds[deptIndex]]));
            }
        }

        // Act
        var resultZero = await GetTopDepartments(0);

        var resultNegative = await GetTopDepartments(-5);

        // Assert
        Assert.True(resultZero.IsSuccess);
        Assert.Equal(5, resultZero.Value.TopDepartmentsByPositionsCount.Count);

        Assert.True(resultNegative.IsSuccess);
        Assert.Equal(5, resultNegative.Value.TopDepartmentsByPositionsCount.Count);
    }
}
using DirectoryService.Contracts.Departments.GetDepartmentChildren;
using DirectoryService.Contracts.Locations.CreateLocation;

namespace DirectoryService.IntegrationTests.Departments;

public class GetDepartmentChildrenTests : DepartmentBaseTests
{
    public GetDepartmentChildrenTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetDepartmentChildren_should_return_all_children_with_correct_has_more_children_flag()
    {
        // Arrange
        var locationId = await CreateLocation(new CreateLocationRequest(
            "LocationOne",
            new CreateLocationAddressDto("CityOne", "StreetOne", "One", "SixZeroZeroZeroOne"),
            "Europe/Test"));

        var parent = await CreateDepartment("ParentDept", "parent-dept", [locationId.Value]);
        Assert.True(parent.IsSuccess);

        var childWithoutGrandchildren = await CreateDepartment(
            "ChildAlpha",
            "child-alpha",
            [locationId.Value],
            parent.Value);
        Assert.True(childWithoutGrandchildren.IsSuccess);

        var childWithGrandchildren = await CreateDepartment(
            "ChildBeta",
            "child-beta",
            [locationId.Value],
            parent.Value);
        Assert.True(childWithGrandchildren.IsSuccess);

        await CreateDepartment("GrandchildAlpha", "grandchild-alpha", [locationId.Value], childWithGrandchildren.Value);
        await CreateDepartment("GrandchildBeta", "grandchild-beta", [locationId.Value], childWithGrandchildren.Value);

        // Act
        var result = await GetDepartmentChildren(
            parent.Value,
            new GetDepartmentChildrenRequest(Page: 1, PageSize: 10));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Departments.Count);

        var childAlpha = result.Value.Departments.First(c => c.Name == "ChildAlpha");
        Assert.False(childAlpha.HasMoreChildren);

        // Проверяем ребенка С внуками
        var childBeta = result.Value.Departments.First(c => c.Name == "ChildBeta");
        Assert.True(childBeta.HasMoreChildren);
    }

    [Fact]
    public async Task GetDepartmentChildren_with_pagination_should_return_correct_page()
    {
        // Arrange
        var locationId = await CreateLocation(new CreateLocationRequest(
            "LocationOne",
            new CreateLocationAddressDto("CityOne", "StreetOne", "One", "SixZeroZeroZeroOne"),
            "Europe/Test"));

        var parent = await CreateDepartment("ParentDept", "parent-dept", [locationId.Value]);
        Assert.True(parent.IsSuccess);

        string[] childrenNames =
            ["ChildAlpha", "ChildBeta", "ChildGamma", "ChildDelta", "ChildEpsilon", "ChildZeta", "ChildEta"];
        string[] childrenCodes =
            ["child-alpha", "child-beta", "child-gamma", "child-delta", "child-epsilon", "child-zeta", "child-eta"];

        for (int i = 0; i < 7; i++)
        {
            var child = await CreateDepartment(childrenNames[i], childrenCodes[i], [locationId.Value], parent.Value);
            Assert.True(child.IsSuccess);
        }

        // Act
        var resultPage1 = await GetDepartmentChildren(
            parent.Value,
            new GetDepartmentChildrenRequest(Page: 1, PageSize: 3));

        var resultPage2 = await GetDepartmentChildren(
            parent.Value,
            new GetDepartmentChildrenRequest(Page: 2, PageSize: 3));

        var resultPage3 = await GetDepartmentChildren(
            parent.Value,
            new GetDepartmentChildrenRequest(Page: 3, PageSize: 3));

        // Assert
        Assert.True(resultPage1.IsSuccess);
        Assert.Equal(3, resultPage1.Value.Departments.Count);

        Assert.True(resultPage2.IsSuccess);
        Assert.Equal(3, resultPage2.Value.Departments.Count);

        Assert.True(resultPage3.IsSuccess);
        Assert.Single(resultPage3.Value.Departments);

        var page1Names = resultPage1.Value.Departments.Select(c => c.Name).ToList();
        var page2Names = resultPage2.Value.Departments.Select(c => c.Name).ToList();

        Assert.DoesNotContain(page2Names, name => page1Names.Contains(name));
    }

    [Fact]
    public async Task GetDepartmentChildren_with_nonexistent_parent_should_return_empty_list()
    {
        // Arrange
        var nonExistentParentId = Guid.NewGuid();

        // Act
        var result = await GetDepartmentChildren(
            nonExistentParentId,
            new GetDepartmentChildrenRequest(Page: 1, PageSize: 10));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Departments);
    }

    [Fact]
    public async Task GetDepartmentChildren_with_default_parameters_should_use_defaults()
    {
        // Arrange
        var locationId = await CreateLocation(new CreateLocationRequest(
            "LocationOne",
            new CreateLocationAddressDto("CityOne", "StreetOne", "One", "SixZeroZeroZeroOne"),
            "Europe/Test"));

        var parent = await CreateDepartment("ParentDept", "parent-dept", [locationId.Value]);
        Assert.True(parent.IsSuccess);

        string[] childrenNames = ["ChildAlpha", "ChildBeta", "ChildGamma", "ChildDelta", "ChildEpsilon"];
        string[] childrenCodes = ["child-alpha", "child-beta", "child-gamma", "child-delta", "child-epsilon"];

        for (int i = 0; i < 5; i++)
        {
            await CreateDepartment(childrenNames[i], childrenCodes[i], [locationId.Value], parent.Value);
        }

        // Act
        var resultWithZero = await GetDepartmentChildren(
            parent.Value,
            new GetDepartmentChildrenRequest(Page: 0, PageSize: 0));

        var resultWithNull = await GetDepartmentChildren(
            parent.Value,
            new GetDepartmentChildrenRequest(Page: null, PageSize: null));

        // Assert
        Assert.True(resultWithZero.IsSuccess);
        Assert.Equal(5, resultWithZero.Value.Departments.Count);

        Assert.True(resultWithNull.IsSuccess);
        Assert.Equal(5, resultWithNull.Value.Departments.Count);
    }
}
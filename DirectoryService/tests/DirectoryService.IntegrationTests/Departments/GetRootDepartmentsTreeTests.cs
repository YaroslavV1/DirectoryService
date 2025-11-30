using DirectoryService.Contracts.Departments.GetRootDepartmentsTree;
using DirectoryService.Contracts.Locations.CreateLocation;

namespace DirectoryService.IntegrationTests.Departments;

public class GetRootDepartmentsTreeTests : DepartmentBaseTests
{
    public GetRootDepartmentsTreeTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetRootDepartmentsTree_should_return_roots_with_prefetched_children()
    {
        // Arrange
        var locationId = await CreateLocation(new CreateLocationRequest(
            "LocationOne",
            new CreateLocationAddressDto("CityOne", "StreetOne", "One", "SixZeroZeroZeroOne"),
            "Europe/Test"));

        var root1 = await CreateDepartment("RootAlpha", "root-alpha", [locationId.Value]);
        var root2 = await CreateDepartment("RootBeta", "root-beta", [locationId.Value]);

        Assert.True(root1.IsSuccess);
        Assert.True(root2.IsSuccess);

        string[] childrenNames = ["ChildAlpha", "ChildBeta", "ChildGamma", "ChildDelta", "ChildEpsilon"];
        string[] childrenCodes = ["child-alpha", "child-beta", "child-gamma", "child-delta", "child-epsilon"];

        for (int i = 0; i < 5; i++)
        {
            await CreateDepartment(childrenNames[i], childrenCodes[i], [locationId.Value], root1.Value);
        }

        await CreateDepartment("ChildZeta", "child-zeta", [locationId.Value], root2.Value);
        await CreateDepartment("ChildEta", "child-eta", [locationId.Value], root2.Value);

        // Act
        var result = await GetRootDepartmentsTree(new GetRootDepartmentsTreeRequest(
            Page: 1,
            PageSize: 10,
            Prefetch: 3));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.RootDepartmentsTree.Count);

        var firstRoot = result.Value.RootDepartmentsTree.First(d => d.Name == "RootAlpha");
        Assert.Equal(3, firstRoot.Children.Count);
        Assert.True(firstRoot.HasMoreChildren);

        var secondRoot = result.Value.RootDepartmentsTree.First(d => d.Name == "RootBeta");
        Assert.Equal(2, secondRoot.Children.Count);
        Assert.False(secondRoot.HasMoreChildren);
    }

    [Fact]
    public async Task GetRootDepartmentsTree_with_pagination_should_return_correct_page()
    {
        // Arrange
        var locationId = await CreateLocation(new CreateLocationRequest(
            "LocationOne",
            new CreateLocationAddressDto("CityOne", "StreetOne", "One", "SixZeroZeroZeroOne"),
            "Europe/Test"));

        string[] rootNames = ["RootAlpha", "RootBeta", "RootGamma", "RootDelta", "RootEpsilon", "RootZeta", "RootEta"];
        string[] rootCodes =
            ["root-alpha", "root-beta", "root-gamma", "root-delta", "root-epsilon", "root-zeta", "root-eta"];

        for (int i = 0; i < 7; i++)
        {
            var root = await CreateDepartment(rootNames[i], rootCodes[i], [locationId.Value]);
            Assert.True(root.IsSuccess);
        }

        // Act
        var resultPage1 = await GetRootDepartmentsTree(new GetRootDepartmentsTreeRequest(
            Page: 1,
            PageSize: 3,
            Prefetch: 2));

        // Act
        var resultPage2 = await GetRootDepartmentsTree(new GetRootDepartmentsTreeRequest(
            Page: 2,
            PageSize: 3,
            Prefetch: 2));

        // Assert
        Assert.True(resultPage1.IsSuccess);
        Assert.Equal(3, resultPage1.Value.RootDepartmentsTree.Count);

        Assert.True(resultPage2.IsSuccess);
        Assert.Equal(3, resultPage2.Value.RootDepartmentsTree.Count);
    }

    [Fact]
    public async Task GetRootDepartmentsTree_children_should_have_correct_has_more_children_flag()
    {
        // Arrange
        var locationId = await CreateLocation(new CreateLocationRequest(
            "LocationOne",
            new CreateLocationAddressDto("CityOne", "StreetOne", "One", "SixZeroZeroZeroOne"),
            "Europe/Test"));

        var root = await CreateDepartment("RootAlpha", "root-alpha", [locationId.Value]);
        Assert.True(root.IsSuccess);

        var child1 = await CreateDepartment("ChildAlpha", "child-alpha", [locationId.Value], root.Value);
        Assert.True(child1.IsSuccess);

        var child2 = await CreateDepartment("ChildBeta", "child-beta", [locationId.Value], root.Value);
        Assert.True(child2.IsSuccess);

        await CreateDepartment("GrandchildAlpha", "grandchild-alpha", [locationId.Value], child2.Value);
        await CreateDepartment("GrandchildBeta", "grandchild-beta", [locationId.Value], child2.Value);

        // Act
        var result = await GetRootDepartmentsTree(new GetRootDepartmentsTreeRequest(
            Page: 1,
            PageSize: 10,
            Prefetch: 5));

        // Assert
        Assert.True(result.IsSuccess);

        var rootDept = result.Value.RootDepartmentsTree.First();
        Assert.Equal(2, rootDept.Children.Count);

        // Первый ребенок без детей
        var firstChild = rootDept.Children.First(c => c.Name == "ChildAlpha");
        Assert.False(firstChild.HasMoreChildren);

        // Второй ребенок с детьми
        var secondChild = rootDept.Children.First(c => c.Name == "ChildBeta");
        Assert.True(secondChild.HasMoreChildren);
    }

    [Fact]
    public async Task GetRootDepartmentsTree_with_no_roots_should_return_empty_list()
    {
        // Arrange

        // Act
        var result = await GetRootDepartmentsTree(new GetRootDepartmentsTreeRequest(
            Page: 1,
            PageSize: 10,
            Prefetch: 3));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.RootDepartmentsTree);
    }

    [Fact]
    public async Task GetRootDepartmentsTree_with_default_parameters_should_use_defaults()
    {
        // Arrange
        var locationId = await CreateLocation(new CreateLocationRequest(
            "LocationOne",
            new CreateLocationAddressDto("CityOne", "StreetOne", "One", "SixZeroZeroZeroOne"),
            "Europe/Test"));

        var root = await CreateDepartment("RootAlpha", "root-alpha", [locationId.Value]);
        Assert.True(root.IsSuccess);

        string[] childrenNames = ["ChildAlpha", "ChildBeta", "ChildGamma", "ChildDelta", "ChildEpsilon"];
        string[] childrenCodes = ["child-alpha", "child-beta", "child-gamma", "child-delta", "child-epsilon"];

        for (int i = 0; i < 5; i++)
        {
            await CreateDepartment(childrenNames[i], childrenCodes[i], [locationId.Value], root.Value);
        }

        // Act
        var result = await GetRootDepartmentsTree(new GetRootDepartmentsTreeRequest(
            Page: 0,
            PageSize: 0,
            Prefetch: 0));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.RootDepartmentsTree);

        var rootDept = result.Value.RootDepartmentsTree.First();
        Assert.Equal(3, rootDept.Children.Count);
        Assert.True(rootDept.HasMoreChildren);
    }
}
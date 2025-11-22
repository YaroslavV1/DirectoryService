using DirectoryService.Application.Departments.MoveDepartment;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Departments.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Departments;

public class MoveDepartmentTests : DepartmentBaseTests
{
    public MoveDepartmentTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task MoveDepartment_with_valid_data_with_parentId_should_succeed()
    {
        // Arrange
        var locationId = await CreateLocation(
            new CreateLocationRequest(
                "TestLoc1",
                new CreateLocationAddressDto("TestCity1", "av. Street1", "2", "60001"),
                "Europe/Test"));

        var parent1 = await CreateDepartment(
            "ParentDepartment",
            "parent-dev",
            [locationId.Value]);

        var departmentResult = await CreateDepartment(
            "TestDepartment",
            "test-identifier",
            [locationId.Value],
            parent1.Value);

        var futureParent = await CreateDepartment(
            "FutureParent",
            "future",
            [locationId.Value]);

        // Act
        var result = await ExecuteHandler(async (MoveDepartmentHandler sut) =>
        {
            var command = new MoveDepartmentCommand(
                departmentResult.Value,
                new MoveDepartmentRequest(
                    futureParent.Value));

            return await sut.Handle(command);
        });

        // Assert
        Assert.True(result.IsSuccess);
        var department = await ExecuteInDb(async dbContext =>
        {
            return await dbContext.Departments
                .FirstAsync(d => d.Id == DepartmentId.Create(result.Value));
        });

        Assert.Equal("future.test-identifier", department.Path.Value);
        Assert.Equal(1, department.Depth);
    }

    [Fact]
    public async Task MoveDepartment_with_valid_data_without_parentId_should_succeed()
    {
        // Arrange
        var locationId = await CreateLocation(
            new CreateLocationRequest(
                "TestLoc1",
                new CreateLocationAddressDto("TestCity1", "av. Street1", "2", "60001"),
                "Europe/Test"));

        var parent1 = await CreateDepartment(
            "ParentDepartment",
            "parent-dev",
            [locationId.Value]);

        var departmentResult = await CreateDepartment(
            "TestDepartment",
            "test-identifier",
            [locationId.Value],
            parent1.Value);

        // Act
        var result = await ExecuteHandler(async (MoveDepartmentHandler sut) =>
        {
            var command = new MoveDepartmentCommand(
                departmentResult.Value,
                new MoveDepartmentRequest(null));

            return await sut.Handle(command);
        });

        // Assert
        Assert.True(result.IsSuccess);
        var department = await ExecuteInDb(async dbContext =>
        {
            return await dbContext.Departments
                .FirstAsync(d => d.Id == DepartmentId.Create(result.Value));
        });

        Assert.Equal("test-identifier", department.Path.Value);
    }

    [Fact]
    public async Task MoveDepartment_with_nonexisted_department_should_fail()
    {
        // Arrange
        var locationId = await CreateLocation(
            new CreateLocationRequest(
                "TestLoc1",
                new CreateLocationAddressDto("TestCity1", "av. Street1", "2", "60001"),
                "Europe/Test"));

        var departmentResult = Guid.NewGuid();

        var futureParent = await CreateDepartment(
            "FutureParent",
            "future",
            [locationId.Value]);

        // Act
        var result = await ExecuteHandler(async (MoveDepartmentHandler sut) =>
        {
            var command = new MoveDepartmentCommand(
                departmentResult,
                new MoveDepartmentRequest(
                    futureParent.Value));

            return await sut.Handle(command);
        });

        // Assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task MoveDepartment_with_nonexisted_parent_should_fail()
    {
        // Arrange
        var locationId = await CreateLocation(
            new CreateLocationRequest(
                "TestLoc1",
                new CreateLocationAddressDto("TestCity1", "av. Street1", "2", "60001"),
                "Europe/Test"));

        var parent1 = await CreateDepartment(
            "ParentDepartment",
            "parent-dev",
            [locationId.Value]);

        var departmentResult = await CreateDepartment(
            "TestDepartment",
            "test-identifier",
            [locationId.Value],
            parent1.Value);

        var futureParent = Guid.NewGuid();

        // Act
        var result = await ExecuteHandler(async (MoveDepartmentHandler sut) =>
        {
            var command = new MoveDepartmentCommand(
                departmentResult.Value,
                new MoveDepartmentRequest(
                    futureParent));

            return await sut.Handle(command);
        });

        // Assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task MoveDepartment_with_newParent_isDescendant_should_fail()
    {
        // Arrange
        var locationId = await CreateLocation(
            new CreateLocationRequest(
                "TestLoc1",
                new CreateLocationAddressDto("TestCity1", "av. Street1", "2", "60001"),
                "Europe/Test"));

        var parent1 = await CreateDepartment(
            "ParentDepartment",
            "parent-dev",
            [locationId.Value]);

        var departmentResult = await CreateDepartment(
            "TestDepartment",
            "test-identifier",
            [locationId.Value],
            parent1.Value);

        var futureDescendantParent = await CreateDepartment(
            "FutureParent",
            "future",
            [locationId.Value],
            departmentResult.Value);

        // Act
        var result = await ExecuteHandler(async (MoveDepartmentHandler sut) =>
        {
            var command = new MoveDepartmentCommand(
                departmentResult.Value,
                new MoveDepartmentRequest(
                    futureDescendantParent.Value));

            return await sut.Handle(command);
        });

        // Assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task MoveDepartment_to_itself_should_fail()
    {
        // Arrange
        var locationId = await CreateLocation(
            new CreateLocationRequest(
                "TestLoc1",
                new CreateLocationAddressDto("TestCity1", "av. Street1", "2", "60001"),
                "Europe/Test"));

        var department = await CreateDepartment(
            "TestDepartment",
            "test-identifier",
            [locationId.Value]);

        // Act
        var result = await ExecuteHandler(async (MoveDepartmentHandler sut) =>
        {
            var command = new MoveDepartmentCommand(
                department.Value,
                new MoveDepartmentRequest(department.Value)); // сам в себя

            return await sut.Handle(command);
        });

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task MoveDepartment_should_update_descendants_path_and_depth()
    {
        // Arrange
        var locationId = await CreateLocation(
            new CreateLocationRequest(
                "TestLoc1",
                new CreateLocationAddressDto("TestCity1", "av. Street1", "2", "60001"),
                "Europe/Test"));

        var parent = await CreateDepartment(
            "Parent",
            "parent",
            [locationId.Value]);
        var child = await CreateDepartment(
            "Child",
            "child",
            [locationId.Value],
            parent.Value);
        var grandchild = await CreateDepartment(
            "Grandchild",
            "grandchild",
            [locationId.Value],
            child.Value);

        var newParent = await CreateDepartment(
            "NewParent",
            "new-parent",
            [locationId.Value]);

        // Act
        var result = await ExecuteHandler(async (MoveDepartmentHandler sut) =>
        {
            var command = new MoveDepartmentCommand(
                child.Value,
                new MoveDepartmentRequest(newParent.Value));

            return await sut.Handle(command);
        });

        // Assert
        Assert.True(result.IsSuccess);

        var updatedGrandchild = await ExecuteInDb(async dbContext =>
            await dbContext.Departments.FirstAsync(d => d.Id == DepartmentId.Create(grandchild.Value)));

        Assert.Equal("new-parent.child.grandchild", updatedGrandchild.Path.Value);
        Assert.Equal(2, updatedGrandchild.Depth);
    }
}
using DirectoryService.Contracts.Locations.CreateLocation;
using DirectoryService.Contracts.Positions;
using DirectoryService.Domain.Departments.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Departments;

public class SoftDeleteDepartmentTests : DepartmentBaseTests
{
    public SoftDeleteDepartmentTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task SoftDeleteDepartment_should_deactivate_department_and_set_DeletedAt()
    {
        // Arrange
        var locationId = await CreateLocation(new CreateLocationRequest(
            "LocationOne",
            new CreateLocationAddressDto("CityOne", "StreetOne", "One", "600001"),
            "Europe/Test"));

        var departmentResult = await CreateDepartment(
            "TestDepartment",
            "test-dept",
            [locationId.Value]);

        var departmentId = departmentResult.Value;

        // Act
        var deleteResult = await SoftDeleteDepartment(departmentId);

        // Assert
        Assert.True(deleteResult.IsSuccess);

        var department = await ExecuteInDb(async (context) =>
        {
            return await context.Departments
                .FirstOrDefaultAsync(d => d.Id == DepartmentId.Create(departmentId));
        });

        Assert.NotNull(department);
        Assert.False(department.IsActive);
        Assert.True(department.DeletedAt <= DateTime.UtcNow);
    }

    [Fact]
    public async Task SoftDeleteDepartment_should_deactivate_unused_locations_and_positions()
    {
        // Arrange
        var locationId = await CreateLocation(new CreateLocationRequest(
            "ExclusiveLocation",
            new CreateLocationAddressDto("CityOne", "StreetOne", "One", "600001"),
            "Europe/Test"));

        var departmentResult = await CreateDepartment(
            "TestDepartment",
            "test-dept",
            [locationId.Value]);

        var departmentId = departmentResult.Value;

        var positionId = await CreatePosition(new CreatePositionRequest(
            "ExclusivePosition",
            null,
            [departmentId]));

        // Act
        var deleteResult = await SoftDeleteDepartment(departmentId);

        // Assert
        Assert.True(deleteResult.IsSuccess);

        var location = await ExecuteInDb(async (context) =>
        {
            return await context.Locations
                .FirstOrDefaultAsync(l => l.Id == locationId);
        });

        Assert.NotNull(location);
        Assert.False(location.IsActive, "Location used only by deleted department should be deactivated");

        var position = await ExecuteInDb(async (context) =>
        {
            return await context.Positions
                .FirstOrDefaultAsync(p => p.Id == positionId);
        });

        Assert.NotNull(position);
        Assert.False(position.IsActive, "Position used only by deleted department should be deactivated");
    }

    [Fact]
    public async Task SoftDeleteDepartment_should_keep_shared_locations_and_positions_active()
    {
        // Arrange
        var sharedLocationId = await CreateLocation(new CreateLocationRequest(
            "SharedLocation",
            new CreateLocationAddressDto("CityOne", "StreetOne", "One", "600001"),
            "Europe/Test"));

        var department1Result = await CreateDepartment(
            "Department1",
            "dept-one",
            [sharedLocationId.Value]);

        var department2Result = await CreateDepartment(
            "Department2",
            "dept-two",
            [sharedLocationId.Value]);

        var sharedPositionId = await CreatePosition(new CreatePositionRequest(
            "SharedPosition",
            null,
            [department1Result.Value, department2Result.Value]));

        // Act
        var deleteResult = await SoftDeleteDepartment(department1Result.Value);

        // Assert
        Assert.True(deleteResult.IsSuccess);

        var location = await ExecuteInDb(async (context) =>
        {
            return await context.Locations
                .FirstOrDefaultAsync(l => l.Id == sharedLocationId);
        });

        Assert.NotNull(location);
        Assert.True(location.IsActive, "Shared location should remain active");

        var position = await ExecuteInDb(async (context) =>
        {
            return await context.Positions
                .Include(p => p.Departments)
                .FirstOrDefaultAsync(p => p.Id == sharedPositionId);
        });

        Assert.NotNull(position);
        Assert.True(position.IsActive, "Shared position should remain active");
    }

    [Fact]
    public async Task SoftDeleteDepartment_should_update_path_with_deleted_marker_for_all_descendants()
    {
        // Arrange
        var locationId = await CreateLocation(new CreateLocationRequest(
            "LocationOne",
            new CreateLocationAddressDto("CityOne", "StreetOne", "One", "600001"),
            "Europe/Test"));

        var parentResult = await CreateDepartment(
            "ParentDept",
            "parent-dept",
            [locationId.Value]);

        var childResult = await CreateDepartment(
            "ChildDept",
            "child-dept",
            [locationId.Value],
            parentResult.Value);

        var grandchildResult = await CreateDepartment(
            "GrandchildDept",
            "grandchild-dept",
            [locationId.Value],
            childResult.Value);

        var originalParentPath = await ExecuteInDb(async (context) =>
        {
            var dept = await context.Departments
                .FirstOrDefaultAsync(d => d.Id == DepartmentId.Create(parentResult.Value));
            return dept?.Path;
        });

        // Act
        var deleteResult = await SoftDeleteDepartment(parentResult.Value);

        // Assert
        Assert.True(deleteResult.IsSuccess);

        var (deletedParent, child, grandchild) = await ExecuteInDb(async (context) =>
        {
            var parent = await context.Departments
                .FirstOrDefaultAsync(d => d.Id == DepartmentId.Create(parentResult.Value));
            var childDept = await context.Departments
                .FirstOrDefaultAsync(d => d.Id == DepartmentId.Create(childResult.Value));
            var grandchildDept = await context.Departments
                .FirstOrDefaultAsync(d => d.Id == DepartmentId.Create(grandchildResult.Value));

            return (parent, childDept, grandchildDept);
        });

        Assert.NotNull(deletedParent);
        Assert.Contains("deleted", deletedParent.Path.Value, StringComparison.OrdinalIgnoreCase);

        Assert.NotNull(child);
        Assert.Contains("deleted", child.Path.Value, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(deletedParent.Path.Value, child.Path.Value);

        Assert.NotNull(grandchild);
        Assert.Contains("deleted", grandchild.Path.Value, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(deletedParent.Path.Value, grandchild.Path.Value);
    }

    [Fact]
    public async Task SoftDeleteDepartment_should_keep_child_departments_active()
    {
        // Arrange
        var locationId = await CreateLocation(new CreateLocationRequest(
            "LocationOne",
            new CreateLocationAddressDto("CityOne", "StreetOne", "One", "600001"),
            "Europe/Test"));

        var parentResult = await CreateDepartment(
            "ParentDept",
            "parent-dept",
            [locationId.Value]);

        var child1Result = await CreateDepartment(
            "ChildDept1",
            "child-dept-one",
            [locationId.Value],
            parentResult.Value);

        var child2Result = await CreateDepartment(
            "ChildDept2",
            "child-dept-two",
            [locationId.Value],
            parentResult.Value);

        // Act
        var deleteResult = await SoftDeleteDepartment(parentResult.Value);

        // Assert
        Assert.True(deleteResult.IsSuccess);

        var (parent, child1, child2) = await ExecuteInDb(async (context) =>
        {
            var parentDept = await context.Departments
                .FirstOrDefaultAsync(d => d.Id == DepartmentId.Create(parentResult.Value));
            var child1Dept = await context.Departments
                .FirstOrDefaultAsync(d => d.Id == DepartmentId.Create(child1Result.Value));
            var child2Dept = await context.Departments
                .FirstOrDefaultAsync(d => d.Id == DepartmentId.Create(child2Result.Value));

            return (parentDept, child1Dept, child2Dept);
        });

        Assert.NotNull(parent);
        Assert.False(parent.IsActive, "Parent department should be deactivated");

        Assert.NotNull(child1);
        Assert.True(child1.IsActive, "Child department 1 should remain active");

        Assert.NotNull(child2);
        Assert.True(child2.IsActive, "Child department 2 should remain active");
    }
}
using DirectoryService.Application.Departments.DeleteInactiveDepartment;
using DirectoryService.Contracts.Locations.CreateLocation;
using DirectoryService.Contracts.Positions;
using DirectoryService.Domain.Departments.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Departments;

public class DeleteInactiveDepartmentsTests : DepartmentBaseTests
{
    public DeleteInactiveDepartmentsTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task DeleteInactiveDepartment_should_delete_departments_inactive_for_more_than_one_month()
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

        await SoftDeleteDepartment(departmentId);

        // Manually set DeletedAt to more than 1 month ago
        await ExecuteInDb(async (context) =>
        {
            var dept = await context.Departments
                .FirstOrDefaultAsync(d => d.Id == DepartmentId.Create(departmentId));

            if (dept != null)
            {
                var deletedAtField = typeof(Domain.Departments.Department)
                    .GetProperty("DeletedAt");
                deletedAtField?.SetValue(dept, DateTime.UtcNow.AddMonths(-2));
                await context.SaveChangesAsync();
            }
        });

        // Act
        await DeleteInactiveDepartments();

        // Assert
        var department = await ExecuteInDb(async (context) =>
        {
            return await context.Departments
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(d => d.Id == DepartmentId.Create(departmentId));
        });

        Assert.Null(department);
    }

    [Fact]
    public async Task DeleteInactiveDepartment_should_not_delete_recently_deactivated_departments()
    {
        // Arrange
        var locationId = await CreateLocation(new CreateLocationRequest(
            "LocationOne",
            new CreateLocationAddressDto("CityOne", "StreetOne", "One", "600001"),
            "Europe/Test"));

        var departmentResult = await CreateDepartment(
            "RecentlyDeletedDept",
            "recent-dept",
            [locationId.Value]);

        var departmentId = departmentResult.Value;

        await SoftDeleteDepartment(departmentId);

        // Act
        await DeleteInactiveDepartments();

        // Assert
        var department = await ExecuteInDb(async (context) =>
        {
            return await context.Departments
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(d => d.Id == DepartmentId.Create(departmentId));
        });

        Assert.NotNull(department);
        Assert.False(department.IsActive);
    }

    [Fact]
    public async Task DeleteInactiveDepartment_should_reassign_children_to_grandparent()
    {
        // Arrange
        var locationId = await CreateLocation(new CreateLocationRequest(
            "LocationOne",
            new CreateLocationAddressDto("CityOne", "StreetOne", "One", "600001"),
            "Europe/Test"));

        var grandparentResult = await CreateDepartment(
            "GrandparentDept",
            "grand",
            [locationId.Value]);

        var parentResult = await CreateDepartment(
            "ParentDept",
            "parent",
            [locationId.Value],
            grandparentResult.Value);

        var childResult = await CreateDepartment(
            "ChildDept",
            "child",
            [locationId.Value],
            parentResult.Value);

        await SoftDeleteDepartment(parentResult.Value);

        await ExecuteInDb(async (context) =>
        {
            var parent = await context.Departments
                .FirstOrDefaultAsync(d => d.Id == DepartmentId.Create(parentResult.Value));

            if (parent != null)
            {
                var deletedAtField = typeof(Domain.Departments.Department)
                    .GetProperty("DeletedAt");
                deletedAtField?.SetValue(parent, DateTime.UtcNow.AddMonths(-2));
                await context.SaveChangesAsync();
            }
        });

        // Act
        await DeleteInactiveDepartments();

        // Assert
        var child = await ExecuteInDb(async (context) =>
        {
            return await context.Departments
                .FirstOrDefaultAsync(d => d.Id == DepartmentId.Create(childResult.Value));
        });

        var grandparent = await ExecuteInDb(async (context) =>
        {
            return await context.Departments
                .FirstOrDefaultAsync(d => d.Id == DepartmentId.Create(grandparentResult.Value));
        });

        Assert.NotNull(child);
        Assert.NotNull(grandparent);
        Assert.Equal(grandparentResult.Value, child.ParentId?.Value);
        Assert.Contains(grandparent.Path.Value, child.Path.Value);
    }

    [Fact]
    public async Task DeleteInactiveDepartment_should_make_children_root_when_parent_has_no_grandparent()
    {
        // Arrange
        var locationId = await CreateLocation(new CreateLocationRequest(
            "LocationOne",
            new CreateLocationAddressDto("CityOne", "StreetOne", "One", "600001"),
            "Europe/Test"));

        var parentResult = await CreateDepartment(
            "RootParentDept",
            "root-parent-dept",
            [locationId.Value]);

        var childResult = await CreateDepartment(
            "ChildDept",
            "child-dept",
            [locationId.Value],
            parentResult.Value);

        await SoftDeleteDepartment(parentResult.Value);

        await ExecuteInDb(async (context) =>
        {
            var parent = await context.Departments
                .FirstOrDefaultAsync(d => d.Id == DepartmentId.Create(parentResult.Value));

            if (parent != null)
            {
                var deletedAtField = typeof(Domain.Departments.Department)
                    .GetProperty("DeletedAt");
                deletedAtField?.SetValue(parent, DateTime.UtcNow.AddMonths(-2));
                await context.SaveChangesAsync();
            }
        });

        // Act
        await DeleteInactiveDepartments();

        // Assert
        var child = await ExecuteInDb(async (context) =>
        {
            return await context.Departments
                .FirstOrDefaultAsync(d => d.Id == DepartmentId.Create(childResult.Value));
        });

        Assert.NotNull(child);
        Assert.Null(child.ParentId);
        Assert.Equal(0, child.Depth);
    }

    [Fact]
    public async Task DeleteInactiveDepartment_should_delete_inactive_locations_without_active_departments()
    {
        // Arrange
        var exclusiveLocationId = await CreateLocation(new CreateLocationRequest(
            "ExclusiveLocation",
            new CreateLocationAddressDto("CityOne", "StreetOne", "One", "600001"),
            "Europe/Test"));

        var departmentResult = await CreateDepartment(
            "TestDepartment",
            "test-dept",
            [exclusiveLocationId.Value]);

        await SoftDeleteDepartment(departmentResult.Value);

        await ExecuteInDb(async (context) =>
        {
            var dept = await context.Departments
                .FirstOrDefaultAsync(d => d.Id == DepartmentId.Create(departmentResult.Value));

            if (dept != null)
            {
                var deletedAtField = typeof(Domain.Departments.Department)
                    .GetProperty("DeletedAt");
                deletedAtField?.SetValue(dept, DateTime.UtcNow.AddMonths(-2));
                await context.SaveChangesAsync();
            }
        });

        // Act
        await DeleteInactiveDepartments();

        // Assert
        var location = await ExecuteInDb(async (context) =>
        {
            return await context.Locations
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(l => l.Id == exclusiveLocationId);
        });

        Assert.Null(location);
    }

    [Fact]
    public async Task DeleteInactiveDepartment_should_delete_inactive_positions_without_active_departments()
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

        var exclusivePositionId = await CreatePosition(new CreatePositionRequest(
            "ExclusivePosition",
            null,
            [departmentResult.Value]));

        await SoftDeleteDepartment(departmentResult.Value);

        await ExecuteInDb(async (context) =>
        {
            var dept = await context.Departments
                .FirstOrDefaultAsync(d => d.Id == DepartmentId.Create(departmentResult.Value));

            if (dept != null)
            {
                var deletedAtField = typeof(Domain.Departments.Department)
                    .GetProperty("DeletedAt");
                deletedAtField?.SetValue(dept, DateTime.UtcNow.AddMonths(-2));
                await context.SaveChangesAsync();
            }
        });

        // Act
        await DeleteInactiveDepartments();

        // Assert
        var position = await ExecuteInDb(async (context) =>
        {
            return await context.Positions
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == exclusivePositionId);
        });

        Assert.Null(position);
    }

    [Fact]
    public async Task DeleteInactiveDepartment_should_handle_multiple_inactive_departments_with_complex_hierarchy()
    {
        // Arrange
        var locationId = await CreateLocation(new CreateLocationRequest(
            "LocationOne",
            new CreateLocationAddressDto("CityOne", "StreetOne", "One", "600001"),
            "Europe/Test"));

        // создам такую связь: Root -> Parent1 -> Child1
        //              -> Parent2 -> Child2
        var rootResult = await CreateDepartment(
            "RootDept",
            "root-dept",
            [locationId.Value]);

        var parent1Result = await CreateDepartment(
            "ParentOneDept",
            "parentOne-dept",
            [locationId.Value],
            rootResult.Value);

        var child1Result = await CreateDepartment(
            "ChildOneDept",
            "childOne-dept",
            [locationId.Value],
            parent1Result.Value);

        var parent2Result = await CreateDepartment(
            "Parent2Dept",
            "parentTwo-dept",
            [locationId.Value],
            rootResult.Value);

        var child2Result = await CreateDepartment(
            "Child2Dept",
            "childTwo-dept",
            [locationId.Value],
            parent2Result.Value);

        await SoftDeleteDepartment(parent1Result.Value);
        await SoftDeleteDepartment(parent2Result.Value);

        await ExecuteInDb(async (context) =>
        {
            var parent1 = await context.Departments
                .FirstOrDefaultAsync(d => d.Id == DepartmentId.Create(parent1Result.Value));
            var parent2 = await context.Departments
                .FirstOrDefaultAsync(d => d.Id == DepartmentId.Create(parent2Result.Value));

            var deletedAtField = typeof(Domain.Departments.Department)
                .GetProperty("DeletedAt");

            if (parent1 != null)
                deletedAtField?.SetValue(parent1, DateTime.UtcNow.AddMonths(-2));
            if (parent2 != null)
                deletedAtField?.SetValue(parent2, DateTime.UtcNow.AddMonths(-2));

            await context.SaveChangesAsync();
        });

        // Act
        await DeleteInactiveDepartments();

        // Assert
        var (child1, child2, root) = await ExecuteInDb(async (context) =>
        {
            var c1 = await context.Departments
                .FirstOrDefaultAsync(d => d.Id == DepartmentId.Create(child1Result.Value));
            var c2 = await context.Departments
                .FirstOrDefaultAsync(d => d.Id == DepartmentId.Create(child2Result.Value));
            var r = await context.Departments
                .FirstOrDefaultAsync(d => d.Id == DepartmentId.Create(rootResult.Value));

            return (c1, c2, r);
        });

        Assert.NotNull(child1);
        Assert.Equal(rootResult.Value, child1.ParentId?.Value);

        Assert.NotNull(child2);
        Assert.Equal(rootResult.Value, child2.ParentId?.Value);

        Assert.NotNull(root);
        Assert.True(root.IsActive);
    }

    [Fact]
    public async Task DeleteInactiveDepartment_should_not_affect_active_departments()
    {
        // Arrange
        var locationId = await CreateLocation(new CreateLocationRequest(
            "LocationOne",
            new CreateLocationAddressDto("CityOne", "StreetOne", "One", "600001"),
            "Europe/Test"));

        var activeDeptResult = await CreateDepartment(
            "ActiveDept",
            "active-dept",
            [locationId.Value]);

        var inactiveDeptResult = await CreateDepartment(
            "InactiveDept",
            "inactive-dept",
            [locationId.Value]);

        await SoftDeleteDepartment(inactiveDeptResult.Value);

        await ExecuteInDb(async (context) =>
        {
            var dept = await context.Departments
                .FirstOrDefaultAsync(d => d.Id == DepartmentId.Create(inactiveDeptResult.Value));

            if (dept != null)
            {
                var deletedAtField = typeof(Domain.Departments.Department)
                    .GetProperty("DeletedAt");
                deletedAtField?.SetValue(dept, DateTime.UtcNow.AddMonths(-2));
                await context.SaveChangesAsync();
            }
        });

        // Act
        await DeleteInactiveDepartments();

        // Assert
        var activeDept = await ExecuteInDb(async (context) =>
        {
            return await context.Departments
                .FirstOrDefaultAsync(d => d.Id == DepartmentId.Create(activeDeptResult.Value));
        });

        Assert.NotNull(activeDept);
        Assert.True(activeDept.IsActive);

        var inactiveDept = await ExecuteInDb(async (context) =>
        {
            return await context.Departments
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(d => d.Id == DepartmentId.Create(inactiveDeptResult.Value));
        });

        Assert.Null(inactiveDept);
    }
}
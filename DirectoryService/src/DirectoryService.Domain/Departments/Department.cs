using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Departments;

public sealed class Department : Shared.Entity<DepartmentId>
{
    private readonly List<Department> _childrenDepartments = [];
    private readonly List<DepartmentPosition.DepartmentPosition> _positions = [];
    private readonly List<DepartmentLocation.DepartmentLocation> _locations = [];

    // ef core
    private Department(DepartmentId id)
        : base(id)
    {
    }

    private Department(
        DepartmentId departmentId,
        DepartmentName name,
        Identifier identifier,
        DepartmentPath path,
        int depth,
        IEnumerable<DepartmentLocation.DepartmentLocation> locations,
        DepartmentId? parentId = null)
        : base(departmentId)
    {
        Name = name;
        Identifier = identifier;
        ParentId = parentId;
        Path = path;
        Depth = depth;
        ChildrenCount = ChildrenDepartments.Count;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        _locations = locations.ToList();
    }

    public DepartmentName Name { get; private set; } = default!;

    public Identifier Identifier { get; private set; } = default!;

    public DepartmentId? ParentId { get; private set; }

    public DepartmentPath Path { get; private set; } = default!;

    public int Depth { get; private set; }

    public int ChildrenCount { get; private set; }

    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<Department> ChildrenDepartments => _childrenDepartments;

    public IReadOnlyList<DepartmentPosition.DepartmentPosition> Positions => _positions;

    public IReadOnlyList<DepartmentLocation.DepartmentLocation> Locations => _locations;

    public static Result<Department, Error> CreateParent(
        DepartmentName name,
        Identifier identifier,
        IEnumerable<DepartmentLocation.DepartmentLocation> departmentLocations,
        DepartmentId? departmentId = null)
    {
        var departmentLocationList = departmentLocations.ToList();

        if (departmentLocationList.Count == 0)
            return Error.Validation("department.location", "Department locations must contain at least one location");

        var path = DepartmentPath.CreateParent(identifier);

        return new Department(
            departmentId ?? DepartmentId.NewId(),
            name,
            identifier,
            path,
            0,
            departmentLocationList);
    }

    public static Result<Department, Error> CreateChild(
        DepartmentName name,
        Identifier identifier,
        Department parent,
        IEnumerable<DepartmentLocation.DepartmentLocation> departmentLocations,
        DepartmentId? departmentId = null)
    {
        var departmentLocationsList = departmentLocations.ToList();

        if (departmentLocationsList.Count == 0)
        {
            return Error.Validation("department.location", "Department locations must contain at least one location");
        }

        var path = parent.Path.CreateChild(identifier);

        parent.ChildrenCount++;

        return new Department(
            departmentId ?? DepartmentId.NewId(),
            name,
            identifier,
            path,
            parent.Depth + 1,
            departmentLocationsList,
            parent.Id);
    }
}
using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments.ValueObjects;
using SharedService;

namespace DirectoryService.Domain.Departments;

public sealed class Department : SharedService.Entity<DepartmentId>
{
    private readonly List<Department> _childrenDepartments = [];
    private readonly List<DepartmentPosition> _positions = [];
    private readonly List<DepartmentLocation> _locations = [];

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
        IEnumerable<DepartmentLocation> locations,
        DepartmentId? parentId = null)
        : base(departmentId)
    {
        Name = name;
        Identifier = identifier;
        ParentId = parentId;
        Path = path;
        Depth = depth;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        _locations = locations.ToList();
    }

    public DepartmentName Name { get; private set; } = default!;

    public Identifier Identifier { get; private set; } = default!;

    public DepartmentId? ParentId { get; private set; }

    public DepartmentPath Path { get; private set; } = default!;

    public int Depth { get; private set; }

    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public DateTime DeletedAt { get; private set; }

    public IReadOnlyList<Department> ChildrenDepartments => _childrenDepartments;

    public IReadOnlyList<DepartmentPosition> Positions => _positions;

    public IReadOnlyList<DepartmentLocation> Locations => _locations;

    public static Result<Department, Error> CreateParent(
        DepartmentName name,
        Identifier identifier,
        IEnumerable<DepartmentLocation> departmentLocations,
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
        IEnumerable<DepartmentLocation> departmentLocations,
        DepartmentId? departmentId = null)
    {
        var departmentLocationsList = departmentLocations.ToList();

        if (departmentLocationsList.Count == 0)
        {
            return Error.Validation("department.location", "Department locations must contain at least one location");
        }

        var path = parent.Path.CreateChild(identifier);

        return new Department(
            departmentId ?? DepartmentId.NewId(),
            name,
            identifier,
            path,
            parent.Depth + 1,
            departmentLocationsList,
            parent.Id);
    }

    public UnitResult<Error> UpdateParent(Department? newParent)
    {
        if (newParent is null)
        {
            ParentId = null;
            Path = DepartmentPath.CreateParent(Identifier);
            Depth = 0;
        }
        else
        {
            ParentId = newParent.Id;
            Path = newParent.Path.CreateChild(Identifier);
            Depth = newParent.Depth + 1;
        }

        UpdatedAt = DateTime.UtcNow;
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> UpdateLocations(IEnumerable<DepartmentLocation> deparmentLocations)
    {
        if (!deparmentLocations.Any())
        {
            return Error.Validation(
                "department.locations.validation",
                "Department locations must contain at least one location");
        }

        _locations.Clear();
        _locations.AddRange(deparmentLocations);
        UpdatedAt = DateTime.UtcNow;

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> SoftDelete()
    {
        IsActive = false;

        if (!Path.Value.Contains("deleted-"))
        {
            string[] pathParts = Path.Value.Split('.');

            pathParts[^1] = "deleted-" + pathParts[^1];

            var newPath = DepartmentPath.CreateParent(Identifier.Create(pathParts[0]).Value);

            for (int i = 1; i < pathParts.Length; i++)
            {
                newPath = newPath.CreateChild(Identifier.Create(pathParts[i]).Value);
            }

            Path = newPath;
        }

        UpdatedAt = DateTime.UtcNow;
        DeletedAt = DateTime.UtcNow;

        return UnitResult.Success<Error>();
    }
}
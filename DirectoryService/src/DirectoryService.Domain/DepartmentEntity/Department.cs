using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentEntity.ValueObjects;

namespace DirectoryService.Domain.DepartmentEntity;

public class Department
{
    private readonly List<Department> _childDepartments = [];
    private readonly List<DepartmentPosition> _positions = [];
    private readonly List<DepartmentLocation> _locations = [];

    //ef core
    private Department(){}

    private Department(
        Guid departmentId,
        DepartmentName name,
        Identifier identifier,
        Department? parent,
        DepartmentPath path,
        short depth)
    {
        Id = departmentId;
        Name = name;
        Identifier = identifier;
        ParentId = parent?.ParentId;
        Parent = parent;
        Path = path;
        Depth = depth;
        CreatedAt = DateTime.Now;
    }

    public Guid Id { get; }

    public DepartmentName Name { get; private set; }

    public Identifier Identifier { get; private set; }

    public Guid? ParentId { get; private set; }

    public Department? Parent { get; private set; }

    public DepartmentPath Path { get; private set; }

    public short Depth { get; private set; }

    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }


    public IReadOnlyList<Department> ChildDepartments => _childDepartments;

    public IReadOnlyList<DepartmentPosition> Positions => _positions;

    public IReadOnlyList<DepartmentLocation> Locations => _locations;


    public static Result<Department> Create(
        Guid departmentId,
        DepartmentName name,
        Identifier identifier,
        Department? parent,
        DepartmentPath path,
        short depth)
    {
        if(depth < 0)
            return Result.Failure<Department>("The Depth can not be less than zero.");

        return new Department(
            departmentId,
            name,
            identifier,
            parent,
            path,
            depth);
    }
}
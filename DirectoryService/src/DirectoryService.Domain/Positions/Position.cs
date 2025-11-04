using CSharpFunctionalExtensions;
using DirectoryService.Domain.Positions.ValueObjects;
using DirectoryService.Domain.Shared;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Positions;

public class Position : Shared.Entity<PositionId>
{
    private readonly List<DepartmentPosition.DepartmentPosition> _departments = [];

    // ef core
    private Position(PositionId id)
        : base(id)
    {
    }

    private Position(
        PositionId positionId,
        PositionName positionName,
        IEnumerable<DepartmentPosition.DepartmentPosition> departments,
        string? description = null)
        : base(positionId)
    {
        Name = positionName;
        Description = description;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        _departments = departments.ToList();
    }

    public PositionName Name { get; private set; } = default!;

    public string? Description { get; private set; }

    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAt { get; }

    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<DepartmentPosition.DepartmentPosition> Departments => _departments;

    public static Result<Position, Error> Create(
        PositionId positionId,
        PositionName positionName,
        string? description,
        IEnumerable<DepartmentPosition.DepartmentPosition> departmentsPosition)
    {
        if (description == null)
        {
            return new Position(positionId, positionName, departmentsPosition, description);
        }

        if (description.Length > LengthConstants.MAX_POSITION_DESCRIPTION)
        {
            return GeneralErrors.ValueIsInvalid("PositionDescription");
        }

        if (departmentsPosition.ToList().Count == 0)
        {
            return Error.Validation("department.position", "Department locations must contain at least one location");
        }

        return new Position(positionId, positionName, departmentsPosition, description);
    }
}
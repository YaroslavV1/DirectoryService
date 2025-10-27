using CSharpFunctionalExtensions;
using DirectoryService.Domain.Positions.ValueObjects;
using DirectoryService.Domain.Shared;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Positions;

public class Position: Shared.Entity<PositionId>
{
    private readonly List<DepartmentPosition.DepartmentPosition> _departments = [];

    // ef core
    private Position(PositionId id)
    : base(id)
    { }

    private Position(
        PositionId positionId,
        PositionName positionName,
        string description)
    : base(positionId)
    {
        Name = positionName;
        Description = description;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public PositionName Name { get; private set; } = default!;

    public string Description { get; private set; } = default!;

    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<DepartmentPosition.DepartmentPosition> Departments => _departments;

    public static Result<Position, Error> Create(PositionId positionId, PositionName positionName, string description)
    {
        if (description.Length > LengthConstants.MAX_POSITION_DESCRIPTION)
            return GeneralErrors.ValueIsInvalid("PositionDescription");
        return new Position(positionId, positionName, description);
    }

}
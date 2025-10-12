using CSharpFunctionalExtensions;
using DirectoryService.Domain.Modules.DepartmentEntity;
using DirectoryService.Domain.Modules.PositionEntity.ValueObjects;
using DirectoryService.Domain.Shared;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Modules.PositionEntity;

public class Position: Shared.Entity<PositionId>
{
    private readonly List<DepartmentPosition> _departments = [];

    //ef core
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
    }

    public PositionName Name { get; private set; } = default!;

    public string Description { get; private set; } = default!;

    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<DepartmentPosition> Departments => _departments;

    public static Result<Position, Error> Create(PositionId positionId, PositionName positionName, string description)
    {
        if (description.Length > LengthConstants.MAX_POSITION_DESCRIPTION)
            return GeneralErrors.ValueIsInvalid("PositionDescription");
        return new Position(positionId, positionName, description);
    }

}
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Modules.DepartmentEntity;
using DirectoryService.Domain.Modules.PositionEntity.ValueObjects;

namespace DirectoryService.Domain.Modules.PositionEntity;

public class Position: Shared.Entity<PositionId>
{
    private readonly List<DepartmentPosition> _departments = [];

    private const int MAX_DESCRIPTION_LENGTH = 1000;

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
        CreatedAt = DateTime.Now;
    }


    public PositionName Name { get; private set; }

    public string Description { get; private set; }

    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<DepartmentPosition> Departments => _departments;

    public static Result<Position> Create(PositionId positionId, PositionName positionName, string description)
    {
        if (description.Length > MAX_DESCRIPTION_LENGTH)
            return Result.Failure<Position>($"Description must be less than {MAX_DESCRIPTION_LENGTH} characters!");
        return new Position(positionId, positionName, description);
    }

}
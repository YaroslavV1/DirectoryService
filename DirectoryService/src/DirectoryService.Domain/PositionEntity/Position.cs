using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentEntity;
using DirectoryService.Domain.PositionEntity.ValueObjects;

namespace DirectoryService.Domain.PositionEntity;

public class Position
{
    private readonly List<DepartmentPosition> _departments = [];

    private const int MAX_DESCRIPTION_LENGTH = 1000;

    //ef core
    private Position() { }

    private Position(
        Guid positionId,
        PositionName positionName,
        string description)
    {
        Id = positionId;
        Name = positionName;
        Description = description;
        CreatedAt = DateTime.Now;
    }

    public Guid Id { get; }

    public PositionName Name { get; private set; }

    public string Description { get; private set; }

    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<DepartmentPosition> Departments => _departments;

    public static Result<Position> Create(Guid positionId, PositionName positionName, string description)
    {
        if (description.Length > MAX_DESCRIPTION_LENGTH)
            return Result.Failure<Position>($"Description must be less than {MAX_DESCRIPTION_LENGTH} characters!");
        return new Position(positionId, positionName, description);
    }

}
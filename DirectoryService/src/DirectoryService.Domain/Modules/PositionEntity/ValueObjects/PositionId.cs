namespace DirectoryService.Domain.Modules.PositionEntity.ValueObjects;

public record PositionId
{
    public Guid Value { get; }

    private PositionId(Guid value) => Value = value;

    public static PositionId NewId() => new(Guid.NewGuid());

    public static PositionId Create(Guid value) => new(value);
}
namespace DirectoryService.Domain.Positions.ValueObjects;

public record PositionId
{
    private PositionId(Guid value) => Value = value;
    public Guid Value { get; }

    public static PositionId NewId() => new(Guid.NewGuid());

    public static PositionId Create(Guid value) => new(value);
}
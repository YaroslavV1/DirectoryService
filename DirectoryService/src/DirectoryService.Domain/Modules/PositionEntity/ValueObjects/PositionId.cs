namespace DirectoryService.Domain.Modules.PositionEntity.ValueObjects;

public record PositionId(Guid Value)
{
    public static PositionId NewId() => new(Guid.NewGuid());

    public static PositionId Create(Guid value) => new(value);
}
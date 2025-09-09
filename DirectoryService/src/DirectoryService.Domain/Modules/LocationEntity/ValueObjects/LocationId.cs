namespace DirectoryService.Domain.Modules.LocationEntity.ValueObjects;

public record LocationId(Guid Value)
{
    public static LocationId NewId() => new(Guid.NewGuid());

    public static LocationId Create(Guid value) => new(value);
};
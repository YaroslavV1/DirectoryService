namespace DirectoryService.Domain.Modules.LocationEntity.ValueObjects;

public record LocationId
{
    private Guid Value { get; }
    
    private LocationId(Guid value) => Value = value;
    
    public static LocationId NewId() => new(Guid.NewGuid());

    public static LocationId Create(Guid value) => new(value);
};
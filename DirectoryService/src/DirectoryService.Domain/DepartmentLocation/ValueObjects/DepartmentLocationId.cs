namespace DirectoryService.Domain.DepartmentLocation.ValueObjects;

public record DepartmentLocationId
{
    public Guid Value { get; }

    private DepartmentLocationId(Guid value) => Value = value;

    public static DepartmentLocationId NewGuid() => new(Guid.NewGuid());

    public static DepartmentLocationId Create(Guid value) => new(value);
}
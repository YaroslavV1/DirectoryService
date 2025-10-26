namespace DirectoryService.Domain.DepartmentPosition.ValueObjects;

public record DepartmentPositionId
{
    public Guid Value { get; }

    private DepartmentPositionId(Guid value) => Value = value;

    public static DepartmentPositionId NewGuid() => new(Guid.NewGuid());

    public static DepartmentPositionId Create(Guid value) => new(value);
}
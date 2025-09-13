namespace DirectoryService.Domain.Modules.DepartmentEntity.ValueObjects;

public record DepartmentId
{

    public Guid Value { get; }

    private DepartmentId(Guid value) => Value = value;

    public static DepartmentId NewId() => new(Guid.NewGuid());

    public static DepartmentId Create(Guid value) => new(value);
};
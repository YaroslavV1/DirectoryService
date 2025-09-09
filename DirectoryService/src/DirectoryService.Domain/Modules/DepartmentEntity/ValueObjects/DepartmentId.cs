namespace DirectoryService.Domain.Modules.DepartmentEntity.ValueObjects;

public record DepartmentId(Guid Value)
{
    public static DepartmentId NewId() => new(Guid.NewGuid());

    public static DepartmentId Create(Guid value) => new(value);
};
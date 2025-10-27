namespace DirectoryService.Domain.Departments.ValueObjects;

public record DepartmentPath
{
    private const char Separator = '/';

    public string Value { get; }

    private DepartmentPath(string value)
    {
        Value = value;
    }

    public static DepartmentPath CreateParent(Identifier identifier)
    {
        return new DepartmentPath(identifier.Value);
    }

    public DepartmentPath CreateChild(Identifier identifier)
    {
        return new DepartmentPath(Value + Separator + identifier.Value);
    }
}
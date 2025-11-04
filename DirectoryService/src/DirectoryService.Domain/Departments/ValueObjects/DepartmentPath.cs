namespace DirectoryService.Domain.Departments.ValueObjects;

public record DepartmentPath
{
    private const char SEPARATOR = '.';

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
        return new DepartmentPath(Value + SEPARATOR + identifier.Value);
    }
}
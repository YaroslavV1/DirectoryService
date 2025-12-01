namespace DirectoryService.Contracts.Departments.GetDepartmentChildren;

public record DepartmentChildrenDto
{
    public Guid Id { get; init; }

    public Guid? ParentId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Path { get; init; } = string.Empty;

    public int Depth { get; init; }

    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }

    public bool HasMoreChildren { get; set; }
}
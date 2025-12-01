namespace DirectoryService.Contracts.Departments.GetRootDepartmentsTree;

public sealed class RootDepartmentTreeDto
{
    public Guid Id { get; init; }

    public Guid? ParentId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Path { get; init; } = string.Empty;

    public int Depth { get; init; }

    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }

    public List<RootDepartmentTreeDto> Children { get; set; } = [];

    public bool HasMoreChildren { get; set; }
}
namespace DirectoryService.Contracts.Departments.GetTopDepartments;

public record GetTopDepartmentsByPositionsCountDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }

    public int TotalPositions { get; init; }
}
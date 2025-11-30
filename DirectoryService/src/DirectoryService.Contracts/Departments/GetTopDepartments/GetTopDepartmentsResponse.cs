namespace DirectoryService.Contracts.Departments.GetTopDepartments;

public record GetTopDepartmentsResponse(
    IReadOnlyList<GetTopDepartmentsByPositionsCountDto> TopDepartmentsByPositionsCount,
    int TotalDepartments);
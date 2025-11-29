namespace DirectoryService.Contracts.Departments;

public record GetTopDepartmentsResponse(
    IReadOnlyList<GetTopDepartmentsByPositionsCountDto> TopDepartmentsByPositionsCount,
    int TotalDepartments);
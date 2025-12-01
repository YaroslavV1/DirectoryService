namespace DirectoryService.Contracts.Departments.GetTopDepartments;

public record GetTopDepartmentsRequest(
    int? Top = 5);
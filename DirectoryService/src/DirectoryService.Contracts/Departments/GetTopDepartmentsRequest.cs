namespace DirectoryService.Contracts.Departments;

public record GetTopDepartmentsRequest(
    int? Top = 5);
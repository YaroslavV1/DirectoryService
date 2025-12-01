namespace DirectoryService.Contracts.Departments.GetRootDepartmentsTree;

public record GetRootDepartmentsTreeRequest(
    int? Page = 1,
    int? PageSize = 20,
    int? Prefetch = 3);
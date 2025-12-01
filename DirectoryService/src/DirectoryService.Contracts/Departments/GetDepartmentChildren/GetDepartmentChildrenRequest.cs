namespace DirectoryService.Contracts.Departments.GetDepartmentChildren;

public record GetDepartmentChildrenRequest(
    int? Page = 1,
    int? PageSize = 20);
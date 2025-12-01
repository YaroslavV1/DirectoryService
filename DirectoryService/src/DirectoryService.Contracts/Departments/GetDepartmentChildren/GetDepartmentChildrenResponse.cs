namespace DirectoryService.Contracts.Departments.GetDepartmentChildren;

public record GetDepartmentChildrenResponse(
    List<DepartmentChildrenDto> Departments);
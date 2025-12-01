namespace DirectoryService.Contracts.Departments.GetRootDepartmentsTree;

public record RootDepartmentTreeResponse(
    List<RootDepartmentTreeDto> RootDepartmentsTree);
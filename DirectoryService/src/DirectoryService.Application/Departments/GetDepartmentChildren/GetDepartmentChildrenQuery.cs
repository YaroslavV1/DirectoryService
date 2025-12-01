using DirectoryService.Application.Abstractions.Queries;
using DirectoryService.Contracts.Departments.GetDepartmentChildren;

namespace DirectoryService.Application.Departments.GetDepartmentChildren;

public record GetDepartmentChildrenQuery(
    Guid ParentId,
    GetDepartmentChildrenRequest Request) : IQuery;
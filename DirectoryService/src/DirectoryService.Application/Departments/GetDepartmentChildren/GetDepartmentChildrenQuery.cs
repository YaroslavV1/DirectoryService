using DirectoryService.Contracts.Departments.GetDepartmentChildren;
using SharedService.Core.Abstractions.Queries;

namespace DirectoryService.Application.Departments.GetDepartmentChildren;

public record GetDepartmentChildrenQuery(
    Guid ParentId,
    GetDepartmentChildrenRequest Request) : IQuery;
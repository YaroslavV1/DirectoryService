using DirectoryService.Contracts.Departments.GetRootDepartmentsTree;
using SharedService.Core.Abstractions.Queries;

namespace DirectoryService.Application.Departments.GetRootDepartmentsTree;

public record GetRootDepartmentsTreeQuery(
    GetRootDepartmentsTreeRequest Request) : IQuery ;
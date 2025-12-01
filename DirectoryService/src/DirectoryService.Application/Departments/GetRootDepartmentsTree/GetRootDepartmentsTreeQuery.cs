using DirectoryService.Application.Abstractions.Queries;
using DirectoryService.Contracts.Departments.GetRootDepartmentsTree;

namespace DirectoryService.Application.Departments.GetRootDepartmentsTree;

public record GetRootDepartmentsTreeQuery(
    GetRootDepartmentsTreeRequest Request) : IQuery ;
using DirectoryService.Application.Abstractions.Queries;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.Departments.GetTopDepartmentsByPositionCount;

public record GetTopDepartmentsQuery(GetTopDepartmentsRequest Request) : IQuery;
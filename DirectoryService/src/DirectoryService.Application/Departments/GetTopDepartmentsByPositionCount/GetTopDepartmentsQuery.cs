using DirectoryService.Contracts.Departments.GetTopDepartments;
using SharedService.Core.Abstractions.Queries;

namespace DirectoryService.Application.Departments.GetTopDepartmentsByPositionCount;

public record GetTopDepartmentsQuery(GetTopDepartmentsRequest Request) : IQuery;
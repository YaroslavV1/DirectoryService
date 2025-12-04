using DirectoryService.Application.Abstractions.Commands;
using DirectoryService.Application.Abstractions.Queries;

namespace DirectoryService.Application.Departments.DeleteDepartment;

public record SoftDeleteDepartmentCommand(
    Guid DepartmentId): ICommand;
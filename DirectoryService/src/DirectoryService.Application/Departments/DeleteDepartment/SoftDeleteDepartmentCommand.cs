using SharedService.Core.Abstractions.Commands;

namespace DirectoryService.Application.Departments.DeleteDepartment;

public record SoftDeleteDepartmentCommand(
    Guid DepartmentId): ICommand;
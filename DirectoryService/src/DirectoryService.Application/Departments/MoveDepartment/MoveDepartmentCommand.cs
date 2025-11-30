using DirectoryService.Application.Abstractions.Commands;
using DirectoryService.Contracts.Departments.MoveDepartment;

namespace DirectoryService.Application.Departments.MoveDepartment;

public record MoveDepartmentCommand(Guid DepartmentId, MoveDepartmentRequest Request) : ICommand;
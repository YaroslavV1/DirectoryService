using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Commands;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.Departments.MoveDepartment;

public record MoveDepartmentCommand(Guid DepartmentId, MoveDepartmentRequest Request) : ICommand;
using DirectoryService.Contracts.Departments.MoveDepartment;
using SharedService.Core.Abstractions.Commands;

namespace DirectoryService.Application.Departments.MoveDepartment;

public record MoveDepartmentCommand(Guid DepartmentId, MoveDepartmentRequest Request) : ICommand;
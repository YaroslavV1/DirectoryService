using DirectoryService.Contracts.Departments.CreateDepartment;
using SharedService.Core.Abstractions.Commands;

namespace DirectoryService.Application.Departments.CreateDepartment;

public record CreateDepartmentCommand(
    CreateDepartmentRequest Request) : ICommand;
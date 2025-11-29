using DirectoryService.Application.Abstractions.Commands;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.Departments.CreateDepartment;

public record CreateDepartmentCommand(
    CreateDepartmentRequest Request) : ICommand;
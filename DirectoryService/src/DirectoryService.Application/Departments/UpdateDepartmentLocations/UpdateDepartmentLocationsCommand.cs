using System.Windows.Input;
using DirectoryService.Contracts.Departments;
using ICommand = DirectoryService.Application.Abstractions.ICommand;

namespace DirectoryService.Application.Departments.UpdateDepartmentLocations;

public record UpdateDepartmentLocationsCommand(
    Guid DepartmentId,
    UpdateDepartmentLocationsRequest Request) : ICommand;
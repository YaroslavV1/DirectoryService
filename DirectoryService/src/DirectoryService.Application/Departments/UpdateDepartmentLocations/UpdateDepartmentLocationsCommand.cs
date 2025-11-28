using DirectoryService.Application.Abstractions.Commands;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.Departments.UpdateDepartmentLocations;

public record UpdateDepartmentLocationsCommand(
    Guid DepartmentId,
    UpdateDepartmentLocationsRequest Request) : ICommand;
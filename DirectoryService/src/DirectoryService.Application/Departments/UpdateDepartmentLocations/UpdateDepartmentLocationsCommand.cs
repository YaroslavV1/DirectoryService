using DirectoryService.Contracts.Departments.UpdateDepartmentLocations;
using SharedService.Core.Abstractions.Commands;

namespace DirectoryService.Application.Departments.UpdateDepartmentLocations;

public record UpdateDepartmentLocationsCommand(
    Guid DepartmentId,
    UpdateDepartmentLocationsRequest Request) : ICommand;
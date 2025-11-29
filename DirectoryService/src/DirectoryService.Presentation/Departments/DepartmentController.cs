using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions.Commands;
using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Application.Departments.MoveDepartment;
using DirectoryService.Application.Departments.UpdateDepartmentLocations;
using DirectoryService.Contracts.Departments;
using DirectoryService.Presentation.EndpointResults;
using DirectoryService.Shared;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Departments;

[ApiController]
[Route("api/departments")]
public class DepartmentController : ControllerBase
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> Create(
        [FromServices] ICommandHandler<Result<Guid, Errors>, CreateDepartmentCommand> handle,
        [FromBody] CreateDepartmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateDepartmentCommand(request);

        var result = await handle.Handle(command, cancellationToken);

        return result;
    }

    [HttpPatch("{departmentId}/locations")]
    public async Task<EndpointResult<Guid>> UpdateLocations(
        [FromServices] ICommandHandler<Result<Guid, Errors>, UpdateDepartmentLocationsCommand> handle,
        [FromRoute] Guid departmentId,
        [FromBody] UpdateDepartmentLocationsRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateDepartmentLocationsCommand(departmentId, request);

        var result = await handle.Handle(command, cancellationToken);

        return result;
    }

    [HttpPatch("{departmentId}/parent")]
    public async Task<EndpointResult<Guid>> UpdateParent(
        [FromServices] ICommandHandler<Result<Guid, Errors>, MoveDepartmentCommand> handle,
        [FromRoute] Guid departmentId,
        [FromBody] MoveDepartmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new MoveDepartmentCommand(departmentId, request);

        var result = await handle.Handle(command, cancellationToken);

        return result;
    }
}
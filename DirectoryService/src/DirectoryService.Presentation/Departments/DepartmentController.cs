using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Contracts.Departments;
using DirectoryService.Presentation.EndpointResults;
using DirectoryService.Shared;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Departments;

[ApiController]
[Route("api/departments")]
public class DepartmentController: ControllerBase
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
}
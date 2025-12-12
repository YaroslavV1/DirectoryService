using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Application.Departments.DeleteDepartment;
using DirectoryService.Application.Departments.GetDepartmentChildren;
using DirectoryService.Application.Departments.GetRootDepartmentsTree;
using DirectoryService.Application.Departments.GetTopDepartmentsByPositionCount;
using DirectoryService.Application.Departments.MoveDepartment;
using DirectoryService.Application.Departments.UpdateDepartmentLocations;
using DirectoryService.Contracts.Departments.CreateDepartment;
using DirectoryService.Contracts.Departments.GetDepartmentChildren;
using DirectoryService.Contracts.Departments.GetRootDepartmentsTree;
using DirectoryService.Contracts.Departments.GetTopDepartments;
using DirectoryService.Contracts.Departments.MoveDepartment;
using DirectoryService.Contracts.Departments.UpdateDepartmentLocations;
using Microsoft.AspNetCore.Mvc;
using SharedService;
using SharedService.Core.Abstractions.Commands;
using SharedService.Core.Abstractions.Queries;
using SharedService.Framework.EndpointResults;

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

    [HttpGet("top-positions")]
    public async Task<EndpointResult<GetTopDepartmentsResponse>> GetTopDepartments(
        [FromQuery] GetTopDepartmentsRequest request,
        [FromServices] IQueryHandler<Result<GetTopDepartmentsResponse, Errors>, GetTopDepartmentsQuery> handler,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTopDepartmentsQuery(request);

        var result = await handler.Handle(query, cancellationToken);

        return result;
    }

    [HttpGet("roots")]
    public async Task<EndpointResult<RootDepartmentTreeResponse>> GetRootDepartmentsTree(
        [FromQuery] GetRootDepartmentsTreeRequest request,
        [FromServices] IQueryHandler<Result<RootDepartmentTreeResponse, Errors>, GetRootDepartmentsTreeQuery> handler,
        CancellationToken cancellationToken = default)
    {
        var query = new GetRootDepartmentsTreeQuery(request);

        var result = await handler.Handle(query, cancellationToken);

        return result;
    }

    [HttpGet("{parentId:guid}/children")]
    public async Task<EndpointResult<GetDepartmentChildrenResponse>> GetChildren(
        [FromRoute] Guid parentId,
        [FromQuery] GetDepartmentChildrenRequest request,
        [FromServices] IQueryHandler<
            Result<GetDepartmentChildrenResponse, Errors>,
            GetDepartmentChildrenQuery> handler,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDepartmentChildrenQuery(parentId, request);

        var result = await handler.Handle(query, cancellationToken);
        return result;
    }

    [HttpDelete("{departmentId:guid}")]
    public async Task<EndpointResult<Guid>> Delete(
        [FromRoute] Guid departmentId,
        [FromServices] ICommandHandler<Result<Guid, Errors>, SoftDeleteDepartmentCommand> handle,
        CancellationToken cancellationToken = default)
    {
        var command = new SoftDeleteDepartmentCommand(departmentId);

        var result = await handle.Handle(command, cancellationToken);
        return result;
    }
}
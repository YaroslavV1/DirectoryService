using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions.Commands;
using DirectoryService.Application.Abstractions.Queries;
using DirectoryService.Application.Locations.CreateLocation;
using DirectoryService.Application.Locations.GetLocations;
using DirectoryService.Contracts.Locations.CreateLocation;
using DirectoryService.Contracts.Locations.GetLocations;
using DirectoryService.Presentation.EndpointResults;
using DirectoryService.Shared;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Locations;

[ApiController]
[Route("api/locations")]
public class LocationController : ControllerBase
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> Create(
        [FromServices] ICommandHandler<Result<Guid, Errors>, CreateLocationCommand> handle,
        [FromBody] CreateLocationRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateLocationCommand(request);

        var result = await handle.Handle(command, cancellationToken);

        return result;
    }

    [HttpGet]
    public async Task<EndpointResult<GetLocationsDto>> Get(
        [FromQuery] GetLocationsRequest request,
        [FromServices] IQueryHandler<Result<GetLocationsDto, Errors>, GetLocationsQuery> handler,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLocationsQuery(request);

        var result = await handler.Handle(query, cancellationToken);
        return result;
    }
}
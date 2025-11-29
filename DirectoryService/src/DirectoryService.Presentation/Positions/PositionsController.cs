using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions.Commands;
using DirectoryService.Application.Positions.CreatePosition;
using DirectoryService.Contracts.Positions;
using DirectoryService.Presentation.EndpointResults;
using DirectoryService.Shared;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Positions;

[ApiController]
[Route("api/positions")]
public class PositionsController: ControllerBase
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> Create(
        [FromServices] ICommandHandler<Result<Guid, Errors>, CreatePositionCommand> handle,
        [FromBody] CreatePositionRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreatePositionCommand(request);

        var result = await handle.Handle(command, cancellationToken);

        return result;
    }
}
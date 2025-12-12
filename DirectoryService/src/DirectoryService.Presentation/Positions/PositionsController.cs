using CSharpFunctionalExtensions;
using DirectoryService.Application.Positions.CreatePosition;
using DirectoryService.Contracts.Positions;
using Microsoft.AspNetCore.Mvc;
using SharedService;
using SharedService.Core.Abstractions.Commands;
using SharedService.Framework.EndpointResults;

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
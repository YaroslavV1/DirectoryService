using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations.CreateLocation;
using DirectoryService.Contracts.Locations;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Locations;

[ApiController]
[Route("api/[controller]")]
public class LocationController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromServices] ICommandHandler<Guid, CreateLocationCommand> handle,
        [FromBody] CreateLocationRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateLocationCommand(request);

        var result = await handle.Handle(command, cancellationToken);

        return Ok(result);
    }
}
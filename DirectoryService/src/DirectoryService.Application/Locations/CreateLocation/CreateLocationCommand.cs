using DirectoryService.Contracts.Locations.CreateLocation;
using SharedService.Core.Abstractions.Commands;

namespace DirectoryService.Application.Locations.CreateLocation;

public record CreateLocationCommand(
    CreateLocationRequest Request) : ICommand;